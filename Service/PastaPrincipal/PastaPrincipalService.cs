using Azure.Storage.Blobs;
using FluentValidation;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.Validators;

namespace VoxDocs.Services
{

    public interface IPastaPrincipalService
    {
        Task<PastaPrincipalDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<PastaPrincipalDTO>> ObterTodasPorEmpresaIdAsync(Guid empresaId, bool incluirSubPastas = false);
        Task<PastaPrincipalDTO> ObterPastaPrincipalAsync(Guid empresaId);
        Task<PastaPrincipalCreateDTO> CriarPastaPrincipalAsync(PastaPrincipalCreateDTO dto, Guid empresaId);
        Task RemoverPastaPrincipalAsync(Guid id);
    }

    public class PastaPrincipalService : IPastaPrincipalService
    {
        private readonly IPastaPrincipalRepository _repository;
        private readonly IEmpresasContratanteRepository _empresaRepository;
        private readonly IPastaPrincipalValidator _validator;
        private readonly ILogger<PastaPrincipalService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _blobConnectionString;

        public PastaPrincipalService(
            IPastaPrincipalRepository repository,
            IEmpresasContratanteRepository empresaRepository,
            IPastaPrincipalValidator validator,
            ILogger<PastaPrincipalService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _empresaRepository = empresaRepository;
            _validator = validator;
            _logger = logger;
            _configuration = configuration;
            _blobConnectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value;
        }

        public async Task<PastaPrincipalDTO> ObterPorIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obtendo pasta principal por ID: {Id}", id);

                var pasta = await _repository.ObterPorIdAsync(id);
                if (pasta == null)
                {
                    throw new KeyNotFoundException($"Pasta principal com ID {id} não encontrada");
                }

                return new PastaPrincipalDTO
                {
                    Id = pasta.Id,
                    NomePastaPrincipal = pasta.NomePastaPrincipal,
                    DataCriacao = pasta.DataCriacao,
                    EmpresaId = pasta.EmpresaId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pasta principal por ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PastaPrincipalDTO>> ObterTodasPorEmpresaIdAsync(Guid empresaId, bool incluirSubPastas = false)
        {
            try
            {
                _logger.LogInformation("Obtendo todas as pastas principais para empresa ID: {EmpresaId}", empresaId);

                // Verificar se a empresa existe
                var empresa = await _empresaRepository.ObterPorIdAsync(empresaId);
                if (empresa == null)
                {
                    throw new KeyNotFoundException($"Empresa com ID {empresaId} não encontrada");
                }

                var pastas = await _repository.ObterPorEmpresaIdAsync(empresaId, incluirSubPastas);

                return pastas.Select(p => new PastaPrincipalDTO
                {
                    Id = p.Id,
                    NomePastaPrincipal = p.NomePastaPrincipal,
                    DataCriacao = p.DataCriacao,
                    EmpresaId = p.EmpresaId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pastas principais para empresa ID: {EmpresaId}", empresaId);
                throw new ApplicationException("Ocorreu um erro ao obter as pastas principais", ex);
            }
        }

        public async Task<PastaPrincipalDTO> ObterPastaPrincipalAsync(Guid empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo pasta principal para empresa ID: {EmpresaId}", empresaId);

                var pastas = await ObterTodasPorEmpresaIdAsync(empresaId);
                var pastaPrincipal = pastas.FirstOrDefault();

                if (pastaPrincipal == null)
                {
                    throw new KeyNotFoundException($"Nenhuma pasta principal encontrada para a empresa ID {empresaId}");
                }

                return pastaPrincipal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pasta principal para empresa ID: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<PastaPrincipalCreateDTO> CriarPastaPrincipalAsync(PastaPrincipalCreateDTO dto, Guid empresaId)
        {
            try
            {
                _logger.LogInformation("Criando pasta principal para empresa ID: {EmpresaId}", empresaId);

                // Verificação básica da empresa
                var empresaExiste = await _empresaRepository.ExisteAsync(empresaId);
                if (!empresaExiste)
                {
                    throw new ValidationException("Empresa não encontrada");
                }

                var pasta = new PastaPrincipalModel
                {
                    Id = Guid.NewGuid(),
                    NomePastaPrincipal = dto.NomePastaPrincipal?.Trim(),
                    DataCriacao = DateTime.UtcNow,
                    EmpresaId = empresaId
                };

                // Validação simplificada
                var validationResult = _validator.Validate(pasta);
                if (!validationResult.IsValid)
                {
                    _logger.LogError("Falha na validação da pasta principal: {Erros}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new ValidationException(validationResult.Errors);
                }

                var pastaCriada = await _repository.AdicionarAsync(pasta);

                try
                {
                    // Passa o empresaId para o método
                    await CriarEstruturaStorageAsync(pastaCriada.NomePastaPrincipal, pastaCriada.EmpresaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao criar estrutura de armazenamento, mas continuando...");
                }

                return new PastaPrincipalCreateDTO
                {
                    Id = pastaCriada.Id,
                    NomePastaPrincipal = pastaCriada.NomePastaPrincipal,
                    DataCriacao = pastaCriada.DataCriacao
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta principal para empresa ID: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task RemoverPastaPrincipalAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Removendo pasta principal ID: {Id}", id);
                await _repository.RemoverAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover pasta principal ID: {Id}", id);
                throw;
            }
        }

        private async Task CriarEstruturaStorageAsync(string nomePastaPrincipal, Guid empresaId)
        {
            try
            {
                if (string.IsNullOrEmpty(_blobConnectionString))
                    throw new InvalidOperationException("String de conexão não configurada");

                // Obter nome do container da empresa (normalizado)
                var empresa = await _empresaRepository.ObterPorIdAsync(empresaId);
                if (empresa == null)
                    throw new KeyNotFoundException("Empresa não encontrada");

                var containerName = NormalizarNomeContainer(empresa.EmpresaNome);
                var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Verificar se container existe (deveria ter sido criado pelo EmpresaService)
                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogWarning("Container da empresa {Empresa} não encontrado", empresa.EmpresaNome);
                    return;
                }

                // Cria um arquivo oculto para forçar a exibição da hierarquia
                var folderPath = $"{NormalizarNome(nomePastaPrincipal)}/.keep";
                var blobClient = containerClient.GetBlobClient(folderPath);

                if (!await blobClient.ExistsAsync())
                {
                    await blobClient.UploadAsync(new MemoryStream(new byte[0]));
                    _logger.LogInformation("Marcador de pasta principal criado: {Path}", folderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao preparar estrutura para pasta principal");
                throw;
            }
        }

        private string NormalizarNomeContainer(string nome)
        {
            return nome.Trim()
                .ToLower()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Replace(".", "-");
        }

        private string NormalizarNome(string nome)
        {
            return new string(nome
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                .ToArray())
                .Replace(" ", "-")
                .ToLowerInvariant();
        }
    }
}