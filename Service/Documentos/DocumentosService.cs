using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;
using Azure.Storage.Blobs;
using FluentValidation;
using Azure.Storage.Blobs.Models;

namespace VoxDocs.Services
{
    public interface IDocumentosService
    {
        Task<DocumentoModel> UploadDocumentoAsync(DocumentoCreateDTO dto, Stream arquivoStream);
        Task<DocumentoModel> EditarDocumentoAsync(DocumentoUpdateDTO dto, string usuario, bool isAdmin);
        Task<DocumentoDownloadDTO> DownloadDocumentoAsync(Guid id, string usuario, bool isAdmin);
        Task<IEnumerable<DocumentoModel>> ListarDocumentosPorPastaAsync(Guid pastaPrincipalId, Guid? subPastaId, string usuario, bool isAdmin);
        Task<DocumentoModel> ObterDocumentoParaEdicaoAsync(Guid id, string usuario, bool isAdmin);
        Task<DocumentoDetalhesDTO> ObterDetalhesDocumentoAsync(Guid id, string usuario, bool isAdmin);
        Task RemoverDocumentoAsync(Guid id, string usuario, string tokenAcesso = null);
    }

    public class DocumentosService : IDocumentosService
    {
        private readonly IDocumentoRepository _documentoRepository;
        private readonly IPastaPrincipalRepository _pastaPrincipalRepository;
        private readonly ISubPastaRepository _subPastaRepository;
        private readonly IEmpresasContratanteRepository _empresaRepository; // Adicionado
        private readonly IValidator<DocumentoModel> _validator;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DocumentosService> _logger;

        public DocumentosService(
            IDocumentoRepository documentoRepository,
            IPastaPrincipalRepository pastaPrincipalRepository,
            ISubPastaRepository subPastaRepository,
            IEmpresasContratanteRepository empresaRepository, // Adicionado
            IValidator<DocumentoModel> validator,
            BlobServiceClient blobServiceClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DocumentosService> logger)
        {
            _documentoRepository = documentoRepository;
            _pastaPrincipalRepository = pastaPrincipalRepository;
            _subPastaRepository = subPastaRepository;
            _empresaRepository = empresaRepository; // Adicionado
            _validator = validator;
            _blobServiceClient = blobServiceClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<DocumentoModel> UploadDocumentoAsync(DocumentoCreateDTO dto, Stream arquivoStream)
        {
            try
            {
                _logger.LogInformation("Iniciando upload de documento para o usuário: {Usuario}", dto.UsuarioCriacao);

                if (dto == null || arquivoStream == null || arquivoStream.Length == 0)
                {
                    throw new ArgumentException("Dados inválidos para upload");
                }

                var pastaPrincipal = await _pastaPrincipalRepository.ObterPorIdAsync(dto.IdPastaPrincipal);
                if (pastaPrincipal == null)
                {
                    throw new KeyNotFoundException("Pasta principal não encontrada");
                }

                if (dto.IdSubPasta.HasValue)
                {
                    var subPasta = await _subPastaRepository.ObterPorIdAsync(dto.IdSubPasta.Value);
                    if (subPasta == null || subPasta.PastaPrincipalId != dto.IdPastaPrincipal)
                    {
                        throw new KeyNotFoundException("Subpasta inválida");
                    }
                }

                // Obter o nome do container da empresa
                var empresa = await _empresaRepository.ObterPorIdAsync(pastaPrincipal.EmpresaId);
                if (empresa == null)
                {
                    throw new KeyNotFoundException("Empresa não encontrada");
                }

                var containerName = NormalizarNomeContainer(empresa.EmpresaNome);

                var fileExtension = Path.GetExtension(dto.NomeArquivo).ToLowerInvariant();
                var contentType = GetContentType(fileExtension);

                var safeFileName = Path.GetFileNameWithoutExtension(dto.NomeArquivo)
                    .Replace(" ", "_")
                    .ToLowerInvariant();
                var uniqueFileName = $"{safeFileName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{fileExtension}";

                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                // Definir o caminho do blob com a estrutura de pastas
                string blobPath;
                if (dto.IdSubPasta.HasValue)
                {
                    var subPasta = await _subPastaRepository.ObterPorIdAsync(dto.IdSubPasta.Value);
                    blobPath = $"{NormalizarNome(pastaPrincipal.NomePastaPrincipal)}/{NormalizarNome(subPasta.NomeSubPasta)}/{uniqueFileName}";
                }
                else
                {
                    blobPath = $"{NormalizarNome(pastaPrincipal.NomePastaPrincipal)}/{uniqueFileName}";
                }

                var blobClient = blobContainerClient.GetBlobClient(blobPath);

                // Configurar as opções de upload com o ContentType correto
                var blobUploadOptions = new BlobUploadOptions()
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                await blobClient.UploadAsync(arquivoStream, blobUploadOptions);

                var documento = new DocumentoModel
                {
                    NomeArquivo = Path.GetFileNameWithoutExtension(dto.NomeArquivo),
                    DescricaoArquivo = dto.DescricaoArquivo,
                    IdNomeEmpresa = pastaPrincipal.EmpresaId,
                    IdPastaPrincipal = dto.IdPastaPrincipal,
                    IdSubPasta = dto.IdSubPasta,
                    NivelPrioridade = dto.NivelPrioridade,
                    TamanhoDocumento = arquivoStream.Length,
                    UsuarioCriacao = dto.UsuarioCriacao,
                    TokenAcesso = dto.NivelPrioridade == 3 ? dto.TokenAcesso : null,
                    BlobName = blobPath,
                    ContentType = contentType
                };

                await _validator.ValidateAndThrowAsync(documento);
                return await _documentoRepository.AdicionarAsync(documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload de documento");
                throw;
            }
        }

        public async Task<DocumentoDownloadDTO> DownloadDocumentoAsync(Guid id, string usuario, bool isAdmin)
        {
            try
            {
                var documento = await _documentoRepository.ObterPorIdAsync(id);
                if (documento == null)
                {
                    throw new KeyNotFoundException("Documento não encontrado");
                }

                if (!isAdmin && documento.UsuarioCriacao != usuario && documento.NivelPrioridade > 1)
                {
                    throw new UnauthorizedAccessException("Acesso não autorizado a este documento");
                }

                // Obter o nome do container da empresa
                var empresa = await _empresaRepository.ObterPorIdAsync(documento.IdNomeEmpresa);
                if (empresa == null)
                {
                    throw new KeyNotFoundException("Empresa não encontrada");
                }

                var containerName = NormalizarNomeContainer(empresa.EmpresaNome);

                // Obter a extensão do arquivo original
                var fileExtension = Path.GetExtension(documento.BlobName).ToLowerInvariant();

                // Se não tiver extensão, tentar obter do content type
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = documento.ContentType switch
                    {
                        "application/pdf" => ".pdf",
                        "application/msword" => ".doc",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                        "application/vnd.ms-excel" => ".xls",
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                        "application/vnd.ms-powerpoint" => ".ppt",
                        "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        _ => string.Empty
                    };
                }

                // Lista de extensões permitidas
                var allowedExtensions = new[] {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".jpg", ".jpeg", ".png", ".txt", ".csv"
            };

                if (!string.IsNullOrEmpty(fileExtension) && !allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException($"Tipo de arquivo '{fileExtension}' não permitido para download");
                }

                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(documento.BlobName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new KeyNotFoundException("Arquivo não encontrado no armazenamento");
                }

                var downloadInfo = await blobClient.DownloadAsync();
                using var memoryStream = new MemoryStream();
                await downloadInfo.Value.Content.CopyToAsync(memoryStream);

                return new DocumentoDownloadDTO
                {
                    NomeArquivo = $"{documento.NomeArquivo}{fileExtension}",
                    Conteudo = memoryStream.ToArray(),
                    TipoConteudo = documento.ContentType ?? "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer download de documento");
                throw;
            }
        }

        // Método para normalizar o nome do container
        private string NormalizarNomeContainer(string nomeEmpresa)
        {
            if (string.IsNullOrWhiteSpace(nomeEmpresa))
            {
                throw new ArgumentException("Nome da empresa não pode ser vazio", nameof(nomeEmpresa));
            }

            return new string(nomeEmpresa
                .ToLowerInvariant()
                .Where(c => char.IsLetterOrDigit(c) || c == '-')
                .ToArray())
                .Replace(" ", "-");
        }
        private string NormalizarNome(string nome)
        {
            return new string(nome?
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                .ToArray())
                .Replace(" ", "-")
                .ToLowerInvariant();
        }
        public async Task<DocumentoModel> EditarDocumentoAsync(DocumentoUpdateDTO dto, string usuario, bool isAdmin)
        {
            try
            {
                var documento = await _documentoRepository.ObterPorIdAsync(dto.Id);
                if (documento == null)
                {
                    throw new KeyNotFoundException("Documento não encontrado");
                }

                if (!isAdmin && documento.UsuarioCriacao != usuario)
                {
                    throw new UnauthorizedAccessException("Apenas o criador ou administradores podem editar este documento");
                }

                // Lógica para documentos de alta prioridade (níveis 3+)
                if (documento.NivelPrioridade >= 3 || dto.NivelPrioridade >= 3)
                {
                    // Validação do token atual é obrigatória
                    if (string.IsNullOrEmpty(dto.TokenAtual))
                    {
                        throw new ArgumentException("Token atual é obrigatório para documentos com prioridade alta (níveis 3+)");
                    }

                    if (!ValidarToken(documento.TokenAcesso, dto.TokenAtual))
                    {
                        throw new UnauthorizedAccessException("Token atual inválido");
                    }

                    // Novo token é opcional para níveis 3+
                    if (!string.IsNullOrEmpty(dto.NovoToken))
                    {
                        documento.TokenAcesso = dto.NovoToken;
                    }
                }
                // Lógica para documentos restritos (nível 2)
                else if (dto.NivelPrioridade == 2)
                {
                    // Para nível 2, usa apenas o token atual (não permite definir novo token)
                    if (string.IsNullOrEmpty(dto.TokenAtual))
                    {
                        throw new ArgumentException("Token atual é obrigatório para documentos restritos (nível 2)");
                    }

                    if (!ValidarToken(documento.TokenAcesso, dto.TokenAtual))
                    {
                        throw new UnauthorizedAccessException("Token atual inválido");
                    }
                }
                // Nível 1 (Público) - remove qualquer token
                else
                {
                    documento.TokenAcesso = null;
                }

                // Atualiza os demais campos
                documento.DescricaoArquivo = dto.DescricaoArquivo ?? documento.DescricaoArquivo;
                documento.NivelPrioridade = dto.NivelPrioridade;

                // Atualiza a data de modificação se o modelo tiver essa propriedade
                if (documento.GetType().GetProperty("DataAtualizacao") != null)
                {
                    documento.GetType().GetProperty("DataAtualizacao").SetValue(documento, DateTime.UtcNow);
                }

                await _validator.ValidateAndThrowAsync(documento);
                return await _documentoRepository.AtualizarAsync(documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar documento");
                throw;
            }
        }

        // Método auxiliar para validar token
        private bool ValidarToken(string tokenArmazenado, string tokenFornecido)
        {
            if (string.IsNullOrEmpty(tokenArmazenado) || string.IsNullOrEmpty(tokenFornecido))
                return false;

            return SecureCompare(tokenArmazenado, tokenFornecido);
        }

        // Método auxiliar para comparação segura
        private static bool SecureCompare(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
        public async Task RemoverDocumentoAsync(Guid id, string usuario, string tokenAcesso = null)
        {
            try
            {
                var documento = await _documentoRepository.ObterPorIdAsync(id);
                if (documento == null)
                {
                    throw new KeyNotFoundException("Documento não encontrado");
                }

                var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;

                // Verificar token para documentos confidenciais
                if (documento.NivelPrioridade > 1 && !isAdmin)
                {
                    if (string.IsNullOrEmpty(tokenAcesso))
                    {
                        throw new UnauthorizedAccessException("Token de segurança é requerido para este documento.");
                    }

                    if (documento.TokenAcesso != tokenAcesso)
                    {
                        throw new UnauthorizedAccessException("Token de segurança inválido.");
                    }
                }

                if (!isAdmin && documento.UsuarioCriacao != usuario)
                {
                    throw new UnauthorizedAccessException("Apenas o criador ou administradores podem remover este documento");
                }

                // Obter o nome do container da empresa
                var empresa = await _empresaRepository.ObterPorIdAsync(documento.IdNomeEmpresa);
                if (empresa == null)
                {
                    throw new KeyNotFoundException("Empresa não encontrada");
                }

                var containerName = NormalizarNomeContainer(empresa.EmpresaNome);
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(documento.BlobName);
                await blobClient.DeleteIfExistsAsync();

                await _documentoRepository.RemoverAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover documento");
                throw;
            }
        }
        public async Task<IEnumerable<DocumentoModel>> ListarDocumentosPorPastaAsync(Guid pastaPrincipalId, Guid? subPastaId, string usuario, bool isAdmin)
        {
            try
            {
                var documentos = subPastaId.HasValue
                    ? await _documentoRepository.ObterPorSubPastaAsync(subPastaId.Value)
                    : await _documentoRepository.ObterPorPastaPrincipalAsync(pastaPrincipalId);

                if (!isAdmin)
                {
                    documentos = documentos.Where(d => d.UsuarioCriacao == usuario || d.NivelPrioridade == 1);
                }

                return documentos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar documentos por pasta");
                throw;
            }
        }

        public async Task<DocumentoModel> ObterDocumentoParaEdicaoAsync(Guid id, string usuario, bool isAdmin)
        {
            var documento = await _documentoRepository.ObterPorIdAsync(id);

            if (!isAdmin && documento?.UsuarioCriacao != usuario)
            {
                throw new UnauthorizedAccessException("Apenas o criador ou administradores podem editar este documento");
            }

            return documento;
        }

        public async Task<DocumentoDetalhesDTO> ObterDetalhesDocumentoAsync(Guid id, string usuario, bool isAdmin)
        {
            var documento = await _documentoRepository.ObterPorIdAsync(id);

            if (!isAdmin && documento?.UsuarioCriacao != usuario && documento?.NivelPrioridade > 1)
            {
                throw new UnauthorizedAccessException("Acesso não autorizado a este documento");
            }

            return new DocumentoDetalhesDTO
            {
                Id = documento.Id,
                NomeArquivo = documento.NomeArquivo,
                Descricao = documento.DescricaoArquivo,
                DataCriacao = documento.DataCriacao,
                UsuarioCriacao = documento.UsuarioCriacao,
                NivelPrioridade = documento.NivelPrioridade,
                TamanhoBytes = documento.TamanhoDocumento,
                TipoArquivo = documento.ContentType
            };
        }

        private string GetContentType(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                return "application/octet-stream";

            return fileExtension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}