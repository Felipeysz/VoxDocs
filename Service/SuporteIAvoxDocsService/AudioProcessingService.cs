using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace VoxDocs.Services
{
    public interface IAudioProcessingService
    {
        Task<string> TranscribeAudioAsync(IFormFile audioFile);
        Task<byte[]> GenerateSpeechAsync(string text, string audioType = "regular");
        Task<string> GetChatResponseAsync(string text, string userId);
        bool IsUserAuthenticated();
        string GetUsername();
        Task SaveConversationHistoryAsync(string userId, ConversationHistory history);
        Task<ConversationHistory> GetConversationHistoryAsync(string userId);
        Task ClearConversationHistoryAsync(string userId);
    }

    public class ConversationHistory
    {
        public string UserId { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; }
        public string AudioPath { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsFromUser { get; set; }
    }

    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AudioProcessingService> _logger;
        private readonly string _openAIApiKey;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string AudioBasePath = "audiotemp";
        private const string HistoryBasePath = "conversationhistory";
        private const string TestCompany = "EmpresaTeste";
        private const string WelcomeFileName = "welcome_generic.mp3";
        private static readonly object _welcomeLock = new object();
        private static bool _welcomeAudioGenerated = false;
        private static readonly ConcurrentDictionary<string, ConversationHistory> _activeConversations = new();

        public AudioProcessingService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AudioProcessingService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _openAIApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
            _httpContextAccessor = httpContextAccessor;

            // Garante que os diretórios base existem
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), AudioBasePath));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), HistoryBasePath));
        }

        public bool IsUserAuthenticated()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public string GetUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
        }

        public async Task<string> TranscribeAudioAsync(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new ArgumentException("Arquivo de áudio inválido");
            }

            var username = GetUsername();
            var userAudioDir = GetUserAudioDirectory(username);
            Directory.CreateDirectory(userAudioDir);

            var tempAudioPath = await SaveTempAudioFile(audioFile, userAudioDir);
            try
            {
                return await TranscribeWithWhisper(tempAudioPath, audioFile.FileName, audioFile.ContentType);
            }
            finally
            {
                if (System.IO.File.Exists(tempAudioPath))
                {
                    System.IO.File.Delete(tempAudioPath);
                }
            }
        }

        public async Task<byte[]> GenerateSpeechAsync(string text, string audioType = "regular")
        {
            // Para a mensagem de boas-vindas, verifica se já existe um arquivo cacheado
            if (audioType == "welcome")
            {
                var welcomeAudioPath = GetWelcomeAudioPath();

                // Verifica se o arquivo existe e se já foi gerado nesta instância
                if (System.IO.File.Exists(welcomeAudioPath) && _welcomeAudioGenerated)
                {
                    return await System.IO.File.ReadAllBytesAsync(welcomeAudioPath);
                }

                lock (_welcomeLock)
                {
                    // Verificação dupla dentro do lock para evitar race condition
                    if (System.IO.File.Exists(welcomeAudioPath) && _welcomeAudioGenerated)
                    {
                        return System.IO.File.ReadAllBytes(welcomeAudioPath);
                    }

                    // Gera o áudio de boas-vindas genérico
                    var welcomeText = "Olá! Eu sou a Voxie, assistente virtual da VoxDocs. Como posso te ajudar?";
                    var audioData = ConvertTextToSpeech(welcomeText).GetAwaiter().GetResult();

                    System.IO.File.WriteAllBytes(welcomeAudioPath, audioData);
                    _welcomeAudioGenerated = true;
                    return audioData;
                }
            }

            // Para outros tipos de áudio, processa normalmente
            return await ConvertTextToSpeech(text);
        }

        public async Task<string> GetChatResponseAsync(string text, string userId)
        {
            return await GetChatGptResponse(text, userId);
        }

        public async Task SaveConversationHistoryAsync(string userId, ConversationHistory history)
        {
            try
            {
                // Armazenamento em memória para acesso rápido
                _activeConversations.AddOrUpdate(userId, history, (key, oldValue) => history);

                // Armazenamento persistente em arquivo
                var userHistoryDir = GetUserHistoryDirectory(userId);
                Directory.CreateDirectory(userHistoryDir);

                var historyFilePath = Path.Combine(userHistoryDir, $"history_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                await File.WriteAllTextAsync(historyFilePath, JsonConvert.SerializeObject(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving conversation history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ConversationHistory> GetConversationHistoryAsync(string userId)
        {
            try
            {
                // Primeiro verifica no cache em memória
                if (_activeConversations.TryGetValue(userId, out var cachedHistory))
                {
                    return cachedHistory;
                }

                // Se não encontrou em memória, busca no armazenamento persistente
                var userHistoryDir = GetUserHistoryDirectory(userId);
                if (!Directory.Exists(userHistoryDir))
                {
                    return new ConversationHistory { UserId = userId };
                }

                // Pega o arquivo mais recente
                var latestHistoryFile = Directory.GetFiles(userHistoryDir)
                    .OrderByDescending(f => f)
                    .FirstOrDefault();

                if (latestHistoryFile != null)
                {
                    var historyJson = await File.ReadAllTextAsync(latestHistoryFile);
                    var history = JsonConvert.DeserializeObject<ConversationHistory>(historyJson);

                    // Atualiza o cache em memória
                    _activeConversations.AddOrUpdate(userId, history, (key, oldValue) => history);

                    return history;
                }

                return new ConversationHistory { UserId = userId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversation history for user {UserId}", userId);
                throw;
            }
        }

        public async Task ClearConversationHistoryAsync(string userId)
        {
            try
            {
                // Remove do cache em memória
                _activeConversations.TryRemove(userId, out _);

                // Remove os arquivos persistentes
                var userHistoryDir = GetUserHistoryDirectory(userId);
                if (Directory.Exists(userHistoryDir))
                {
                    Directory.Delete(userHistoryDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation history for user {UserId}", userId);
                throw;
            }
        }

        private async Task<string> SaveTempAudioFile(IFormFile audioFile, string userAudioDir)
        {
            var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(audioFile.FileName)}";
            var tempAudioPath = Path.Combine(userAudioDir, tempFileName);

            await using (var stream = System.IO.File.Create(tempAudioPath))
            {
                await audioFile.CopyToAsync(stream);
            }
            return tempAudioPath;
        }

        private string GetUserAudioDirectory(string username)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), AudioBasePath, TestCompany, username);
        }

        private string GetWelcomeAudioPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), AudioBasePath, WelcomeFileName);
        }

        private string GetUserHistoryDirectory(string userId)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), HistoryBasePath, TestCompany, userId);
        }

        private async Task<string> TranscribeWithWhisper(string audioPath, string originalFileName, string contentType)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                using var content = new MultipartFormDataContent();
                var audioBytes = await System.IO.File.ReadAllBytesAsync(audioPath);
                var audioContent = new ByteArrayContent(audioBytes);

                // Força o tipo de conteúdo para webm se for desconhecido
                var effectiveContentType = contentType.Contains("webm") ? "audio/webm" : contentType;
                audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse(effectiveContentType);

                content.Add(audioContent, "file", originalFileName);
                content.Add(new StringContent("whisper-1"), "model");
                content.Add(new StringContent("text"), "response_format");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAIApiKey);
                request.Content = content;

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                // A API Whisper pode retornar JSON ou texto puro
                if (responseContent.Trim().StartsWith("{") || responseContent.Trim().StartsWith("{"))
                {
                    // É JSON
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    return jsonResponse?.text?.ToString();
                }
                else
                {
                    // É texto direto (isso acontece quando response_format=text)
                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing audio");
                throw new Exception("Failed to transcribe audio", ex);
            }
        }

        private async Task<byte[]> ConvertTextToSpeech(string text)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAIApiKey);

            var requestData = new
            {
                model = "tts-1-hd",
                voice = "nova",
                input = text,  
                response_format = "mp3",
                speed = 1,
            };

            var response = await client.PostAsJsonAsync(
                "https://api.openai.com/v1/audio/speech",
                requestData);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> GetChatGptResponse(string userQuestion, string userId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAIApiKey);

            var messages = new List<object>
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = userQuestion }
            };

            var requestData = new
            {
                model = "gpt-4.1",
                messages = messages,
                max_tokens = 600,
                temperature = 0.7
            };

            var response = await client.PostAsJsonAsync(
                "https://api.openai.com/v1/chat/completions",
                requestData);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            return jsonResponse.choices[0].message.content.ToString();
        }

        private string GetSystemPrompt()
        {
                        return @"Você é a Voxie, assistente virtual da VoxDocs. 
            Sua função é ajudar usuários com a plataforma de gerenciamento de documentos.
            Use linguagem natural e coloquial quando apropriado. 
            Varie a estrutura das frases e use entonação natural. 
            Seja claro, conciso e útil. Mantenha respostas entre 1-3 parágrafos.
            Use ocasionalmente marcadores conversacionais como 'Entendo...', 'Vejamos...', 'Ah, sim...' para soar mais humano.";
        }
    }
}