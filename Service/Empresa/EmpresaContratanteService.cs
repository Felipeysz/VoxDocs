using FluentValidation.Results;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;
using Azure.Storage.Blobs;
using FluentValidation;
using System.Configuration;
using Azure.Storage.Blobs.Models;
using Azure;
using System.Text.RegularExpressions;

namespace VoxDocs.Services
{
    public class EmpresasContratanteService : IEmpresasContratanteService
    {
        private readonly IEmpresasContratanteRepository _empresaRepository;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IValidator<EmpresasContratanteModel> _validator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmpresasContratanteService> _logger;

        public EmpresasContratanteService(
            IEmpresasContratanteRepository empresaRepository,
            IPlanosVoxDocsService planosService,
            IValidator<EmpresasContratanteModel> validator,
            IConfiguration configuration,
            ILogger<EmpresasContratanteService> logger)
        {
            _empresaRepository = empresaRepository;
            _planosService = planosService;
            _validator = validator;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Guid> CriarEmpresaAsync(DTOEmpresaCreate dto)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de nova empresa contratante: {NomeEmpresa}", dto.EmpresaNome);

                // Obter o plano selecionado
                var plano = await _planosService.GetPlanByIdAsync(dto.PlanoId);
                if (plano == null)
                {
                    throw new ArgumentException("Plano selecionado não encontrado");
                }

                // Calcular data de expiração baseada no plano
                DateTime dataExpiracao = plano.Periodicidade switch
                {
                    "Mensal" => DateTime.UtcNow.AddMonths(1),
                    "Trimestral" => DateTime.UtcNow.AddMonths(3),
                    "Semestral" => DateTime.UtcNow.AddMonths(6),
                    "Anual" => DateTime.UtcNow.AddYears(1),
                    "Ilimitado" => DateTime.UtcNow.AddYears(999),
                    _ => DateTime.UtcNow.AddYears(1) // Padrão para caso não encontre
                };

                // Criar modelo
                var empresa = new EmpresasContratanteModel
                {
                    Id = Guid.NewGuid(),
                    EmpresaNome = dto.EmpresaNome?.Trim(),
                    EmailEmpresa = dto.EmailEmpresa?.Trim().ToLower(),
                    DataCriacao = DateTime.UtcNow,
                    PlanoAtivo = true,
                    PlanoId = dto.PlanoId,
                    DataExpiracao = dataExpiracao
                };

                ValidationResult validationResult = await _validator.ValidateAsync(empresa);
                if (!validationResult.IsValid)
                {
                    string errors = string.Join(Environment.NewLine, validationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("Validação falhou para empresa: {Erros}", errors);
                    throw new ValidationException(validationResult.Errors);
                }

                // Persistir a empresa
                EmpresasContratanteModel empresaCriada = await _empresaRepository.AdicionarAsync(empresa);

                // Criar estrutura no Blob Storage
                await CriarEstruturaStorageAsync(empresaCriada.EmpresaNome);

                _logger.LogInformation("Empresa criada com sucesso: {IdEmpresa}", empresaCriada.Id);

                return empresaCriada.Id;
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogError(ex, "Erro de validação ao criar empresa contratante");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar empresa contratante");
                throw new ApplicationException("Ocorreu um erro ao criar a empresa contratante", ex);
            }
        }

        private async Task CriarEstruturaStorageAsync(string nomeEmpresa)
        {
            if (string.IsNullOrWhiteSpace(nomeEmpresa))
            {
                throw new ArgumentException("Nome da empresa não pode ser vazio", nameof(nomeEmpresa));
            }

            try
            {
                // Obtém a string de conexão
                string connectionString = _configuration["AzureBlobStorage:ConnectionString"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationException("String de conexão do Azure Blob Storage não configurada");
                }

                // Cria o cliente do Blob Service
                var blobServiceClient = new BlobServiceClient(connectionString);

                // Normaliza o nome do container (remove caracteres inválidos e deixa em minúsculas)
                string containerName = nomeEmpresa
                    .Trim()
                    .ToLower() // Containers no Azure devem estar em minúsculas
                    .Replace(" ", "-"); // Substitui espaços por hífens (opcional)

                // Remove caracteres inválidos (Azure só permite letras, números e hífens)
                containerName = Regex.Replace(containerName, @"[^a-z0-9-]", "");

                // Verifica se o nome está vazio após a normalização
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    throw new ArgumentException("Nome da empresa não contém caracteres válidos para nome de container");
                }

                // Cria o container específico para a empresa
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Cria o container se não existir (com acesso privado)
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                _logger.LogInformation("Container criado para empresa: {NomeEmpresa} (Container: {ContainerName})",
                    nomeEmpresa, containerName);
            }
            catch (RequestFailedException rfex) when (rfex.Status == 403)
            {
                _logger.LogError(rfex, "Acesso negado ao Azure Blob Storage para empresa: {NomeEmpresa}", nomeEmpresa);
                throw new ApplicationException("Falha de permissão ao acessar o armazenamento", rfex);
            }
            catch (RequestFailedException rfex) when (rfex.Status == 409)
            {
                _logger.LogWarning(rfex, "Container já existia para empresa: {NomeEmpresa}", nomeEmpresa);
                // Não é um erro crítico, pode continuar
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar container no storage para empresa: {NomeEmpresa}", nomeEmpresa);
                throw new ApplicationException("Falha ao criar container de armazenamento para a empresa", ex);
            }
        }

        public async Task<bool> VerificarEmpresaExistenteAsync(string nomeEmpresa, string emailEmpresa)
        {
            try
            {
                _logger.LogInformation("Verificando se empresa já existe - Nome: {NomeEmpresa}, Email: {EmailEmpresa}", 
                    nomeEmpresa, emailEmpresa);

                // Verificar se já existe empresa com o mesmo nome ou email
                bool existeNome = await _empresaRepository.ExistePorAsync(
                    e => e.EmpresaNome == nomeEmpresa.Trim(), 
                    CancellationToken.None);

                bool existeEmail = false;
                
                if (!string.IsNullOrWhiteSpace(emailEmpresa))
                {
                    existeEmail = await _empresaRepository.ExistePorAsync(
                        e => e.EmailEmpresa == emailEmpresa.Trim().ToLower(), 
                        CancellationToken.None);
                }

                return existeNome || existeEmail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se empresa já existe");
                throw new ApplicationException("Ocorreu um erro ao verificar a existência da empresa", ex);
            }
        }

        public async Task<EmpresasContratanteModel> ObterPorNomeAsync(string nomeEmpresa)
        {
            try
            {
                _logger.LogInformation("Obtendo empresa por nome: {NomeEmpresa}", nomeEmpresa);

                var empresa = await _empresaRepository.ObterPorNomeAsync(nomeEmpresa.Trim());

                if (empresa == null)
                {
                    _logger.LogWarning("Empresa não encontrada: {NomeEmpresa}", nomeEmpresa);
                    throw new KeyNotFoundException($"Empresa com nome '{nomeEmpresa}' não encontrada");
                }

                return empresa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter empresa por nome: {NomeEmpresa}", nomeEmpresa);
                throw new ApplicationException("Ocorreu um erro ao buscar a empresa por nome", ex);
            }
        }
    }

    public interface IEmpresasContratanteService
    {
        Task<Guid> CriarEmpresaAsync(DTOEmpresaCreate dto);
        Task<bool> VerificarEmpresaExistenteAsync(string nomeEmpresa, string emailEmpresa);
        Task<EmpresasContratanteModel> ObterPorNomeAsync(string nomeEmpresa);
    }
}