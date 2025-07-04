using Microsoft.AspNetCore.Mvc;
using VoxDocs.Services;
using System.ComponentModel.DataAnnotations;
using static VoxDocs.Services.IAudioProcessingService;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/audio")]
    public class AudioProcessingController : ControllerBase
    {
        private readonly IAudioProcessingService _audioService;
        private readonly ILogger<AudioProcessingController> _logger;
        private static readonly Dictionary<string, bool> _firstInteractionCache = new();

        public AudioProcessingController(
            IAudioProcessingService audioService,
            ILogger<AudioProcessingController> logger)
        {
            _audioService = audioService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessAudio([FromForm] IFormFile audioFile, [FromForm] string userId = null)
        {
            try
            {
                if (!_audioService.IsUserAuthenticated())
                {
                    return Ok(new
                    {
                        status = "unauthenticated",
                        message = "Usuário não está logado"
                    });
                }

                userId ??= _audioService.GetUsername();

                // Verifica o tipo de arquivo
                var allowedTypes = new[] { "audio/webm", "audio/wav", "audio/mpeg", "audio/mp3" };
                if (!allowedTypes.Contains(audioFile.ContentType.ToLower()))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Formato de áudio não suportado. Use WebM, WAV ou MP3."
                    });
                }

                var transcribedText = await _audioService.TranscribeAudioAsync(audioFile);
                var chatResponse = await _audioService.GetChatResponseAsync(transcribedText, userId);
                var audioData = await _audioService.GenerateSpeechAsync(chatResponse);

                return Ok(new
                {
                    status = "success",
                    message = chatResponse,
                    audioData = Convert.ToBase64String(audioData)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio");
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Não foi possível processar o áudio: {ex.Message}"
                });
            }
        }

        [HttpGet("welcome")]
        [HttpHead("welcome")]
        public async Task<IActionResult> GetWelcomeMessage()
        {
            try
            {
                if (!_audioService.IsUserAuthenticated())
                {
                    return Ok(new
                    {
                        status = "unauthenticated",
                        message = "Usuário não está logado"
                    });
                }

                var audioData = await _audioService.GenerateSpeechAsync("", "welcome");

                return Ok(new
                {
                    status = "success",
                    text = "Olá! Eu sou a Voxie, assistente virtual da VoxDocs. Como posso te ajudar?",
                    audioData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating welcome message");
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Não foi possível gerar a mensagem de boas-vindas: {ex.Message}"
                });
            }
        }

        [HttpGet("conversation/history")]
        public async Task<IActionResult> GetConversationHistory([FromQuery] int limit = 50)
        {
            try
            {
                if (!_audioService.IsUserAuthenticated())
                {
                    return Unauthorized(new { status = "unauthenticated", message = "Usuário não está logado" });
                }

                var userId = _audioService.GetUsername();
                var history = await _audioService.GetConversationHistoryAsync(userId);

                return Ok(new
                {
                    status = "success",
                    history = history.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Take(limit)
                        .Reverse(), // To show oldest first
                    lastUpdated = history.LastUpdated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation history");
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Não foi possível recuperar o histórico: {ex.Message}"
                });
            }
        }

        [HttpPost("conversation/save")]
        public async Task<IActionResult> SaveConversation([FromBody] ConversationUpdateRequest request)
        {
            try
            {
                if (!_audioService.IsUserAuthenticated())
                {
                    return Unauthorized(new { status = "unauthenticated", message = "Usuário não está logado" });
                }

                var userId = _audioService.GetUsername();
                var currentHistory = await _audioService.GetConversationHistoryAsync(userId);

                // Update existing messages and add new ones
                foreach (var message in request.Messages)
                {
                    var existing = currentHistory.Messages.FirstOrDefault(m => m.Id == message.Id);
                    if (existing != null)
                    {
                        existing.Text = message.Text;
                        existing.AudioPath = message.AudioPath;
                        existing.Timestamp = message.Timestamp;
                    }
                    else
                    {
                        currentHistory.Messages.Add(message);
                    }
                }

                await _audioService.SaveConversationHistoryAsync(userId, currentHistory);

                return Ok(new
                {
                    status = "success",
                    message = "Conversa salva com sucesso",
                    count = currentHistory.Messages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving conversation");
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Não foi possível salvar a conversa: {ex.Message}"
                });
            }
        }

        [HttpDelete("conversation/clear")]
        public async Task<IActionResult> ClearConversationHistory()
        {
            try
            {
                if (!_audioService.IsUserAuthenticated())
                {
                    return Unauthorized(new { status = "unauthenticated", message = "Usuário não está logado" });
                }

                var userId = _audioService.GetUsername();
                await _audioService.ClearConversationHistoryAsync(userId);

                return Ok(new
                {
                    status = "success",
                    message = "Histórico limpo com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation history");
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Não foi possível limpar o histórico: {ex.Message}"
                });
            }
        }

        public class ConversationUpdateRequest
        {
            [Required]
            public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        }

        private record FirstInteractionResult(string UserId, byte[] AudioData, string Message);
        private record RegularInteractionResult(string UserId, byte[] AudioData, string Message, string TranscribedText);
    }
}