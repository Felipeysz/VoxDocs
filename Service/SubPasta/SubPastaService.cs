using Azure.Storage.Blobs;
using FluentValidation;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public interface ISubPastaService
    {
        Task<PastaPrincipalDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<SubPastaModel>> ObterPorPastaPrincipalIdAsync(Guid pastaPrincipalId);
        Task<SubPastaModel> CriarSubPastaAsync(SubPastaCreateDTO dto);
        Task RemoverSubPastaAsync(Guid id);
        Task<SubPastaModel> ObterSubPastaPorIdAsync(Guid id);
    }

    public class SubPastaService : ISubPastaService
    {
        private readonly ISubPastaRepository _subPastaRepository;
        private readonly IPastaPrincipalRepository _pastaPrincipalRepository;
        private readonly IEmpresasContratanteRepository _empresaRepository;
        private readonly IValidator<SubPastaModel> _validator;
        private readonly ILogger<SubPastaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;

        public SubPastaService(
            ISubPastaRepository subPastaRepository,
            IPastaPrincipalRepository pastaPrincipalRepository,
            IEmpresasContratanteRepository empresaRepository,
            IValidator<SubPastaModel> validator,
            ILogger<SubPastaService> logger,
            IConfiguration configuration,
            BlobServiceClient blobServiceClient)
        {
            _subPastaRepository = subPastaRepository;
            _pastaPrincipalRepository = pastaPrincipalRepository;
            _empresaRepository = empresaRepository;
            _validator = validator;
            _logger = logger;
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<PastaPrincipalDTO> ObterPorIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obtendo pasta principal por ID: {Id}", id);

                var pasta = await _pastaPrincipalRepository.ObterPorIdAsync(id);
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

        public async Task<IEnumerable<SubPastaModel>> ObterPorPastaPrincipalIdAsync(Guid pastaPrincipalId)
        {
            try
            {
                _logger.LogInformation("Obtendo subpastas para pasta principal ID: {PastaPrincipalId}", pastaPrincipalId);

                var pastaPrincipal = await _pastaPrincipalRepository.ObterPorIdAsync(pastaPrincipalId);
                if (pastaPrincipal == null)
                {
                    throw new KeyNotFoundException($"Pasta principal com ID {pastaPrincipalId} não encontrada");
                }

                return await _subPastaRepository.ObterPorPastaPrincipalIdAsync(pastaPrincipalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter subpastas para pasta principal ID: {PastaPrincipalId}", pastaPrincipalId);
                throw new ApplicationException("Ocorreu um erro ao obter as subpastas", ex);
            }
        }

        public async Task<SubPastaModel> CriarSubPastaAsync(SubPastaCreateDTO dto)
        {
            try
            {
                var pastaPrincipal = await _pastaPrincipalRepository.ObterPorIdAsync(dto.PastaPrincipalId);
                if (pastaPrincipal == null)
                {
                    throw new KeyNotFoundException($"Pasta principal com ID {dto.PastaPrincipalId} não encontrada");
                }

                // Criar modelo
                var subPasta = new SubPastaModel
                {
                    Id = Guid.NewGuid(),
                    NomeSubPasta = dto.NomeSubPasta?.Trim(),
                    PastaPrincipalId = dto.PastaPrincipalId,
                    DataCriacao = DateTime.UtcNow
                };

                // Validar modelo
                var validationResult = await _validator.ValidateAsync(subPasta);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Verificar se subpasta já existe
                var subPastasExistentes = await _subPastaRepository.ObterPorPastaPrincipalIdAsync(dto.PastaPrincipalId);
                if (subPastasExistentes.Any(sp => sp.NomeSubPasta.Equals(subPasta.NomeSubPasta, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Já existe uma subpasta com o nome '{subPasta.NomeSubPasta}' nesta pasta principal");
                }

                // Persistir no banco
                var subPastaCriada = await _subPastaRepository.AdicionarAsync(subPasta);

                // Criar estrutura no storage
                await CriarEstruturaStorageAsync(pastaPrincipal.NomePastaPrincipal, subPastaCriada.NomeSubPasta, pastaPrincipal.EmpresaId);

                return subPastaCriada;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar subpasta");
                throw;
            }
        }

        public async Task RemoverSubPastaAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Removendo subpasta ID: {Id}", id);
                await _subPastaRepository.RemoverAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover subpasta ID: {Id}", id);
                throw;
            }
        }

        private async Task CriarEstruturaStorageAsync(string pastaPrincipalNome, string subPastaNome, Guid empresaId)
        {
            try
            {
                _logger.LogInformation("Criando estrutura para: {PastaPrincipal}/{SubPasta}",
                    pastaPrincipalNome, subPastaNome);

                // Obter nome do container da empresa
                var empresa = await _empresaRepository.ObterPorIdAsync(empresaId);
                if (empresa == null)
                {
                    throw new KeyNotFoundException("Empresa não encontrada");
                }

                var containerName = NormalizarNomeContainer(empresa.EmpresaNome);
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogWarning("Container da empresa {Empresa} não encontrado", empresa.EmpresaNome);
                    return;
                }

                // Cria um arquivo vazio para forçar a exibição da hierarquia
                var folderPath = $"{NormalizarNome(pastaPrincipalNome)}/{NormalizarNome(subPastaNome)}/.keep";
                var blobClient = containerClient.GetBlobClient(folderPath);

                if (!await blobClient.ExistsAsync())
                {
                    await blobClient.UploadAsync(new MemoryStream(new byte[0]));
                    _logger.LogInformation("Marcador de estrutura criado: {Path}", folderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar estrutura no storage para subpasta");
                throw new ApplicationException("Falha ao criar estrutura de armazenamento para a subpasta", ex);
            }
        }

        public async Task<SubPastaModel> ObterSubPastaPorIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obtendo subpasta por ID: {Id}", id);

                var subPasta = await _subPastaRepository.ObterPorIdAsync(id);
                if (subPasta == null)
                {
                    throw new KeyNotFoundException($"Subpasta com ID {id} não encontrada");
                }

                return subPasta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter subpasta por ID: {Id}", id);
                throw;
            }
        }

        private string NormalizarNome(string nome)
        {
            return new string(nome?
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                .ToArray())
                .Replace(" ", "-")
                .ToLowerInvariant();
        }

        private string NormalizarNomeContainer(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
            }

            return new string(nome
                .ToLowerInvariant()
                .Where(c => char.IsLetterOrDigit(c) || c == '-')
                .ToArray())
                .Replace(" ", "-");
        }
    }
}