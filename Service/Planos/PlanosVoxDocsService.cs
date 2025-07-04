using FluentValidation;
using Microsoft.Extensions.Logging;
using VoxDocs.BusinessRules;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.Validators;

namespace VoxDocs.Services
{
    public interface IPlanosVoxDocsService
    {
        Task<PlanosVoxDocsModel> GetPlanByIdAsync(Guid id);
        Task<List<PlanosVoxDocsModel>> GetAllPlansAsync();
        Task<PlanosVoxDocsModel> GetPlanByNameAsync(string name);
        Task<PlanosVoxDocsModel> GetPlanByNameAndPeriodicidadeAsync(string nome, string periodicidade);
        Task<List<PlanosVoxDocsModel>> GetPlansByCategoryAsync(string categoria);
        Task<PlanosVoxDocsModel> CreatePlanAsync(DTOPlanosVoxDocs dto);
        Task<PlanosVoxDocsModel> UpdatePlanAsync(Guid id, DTOPlanosVoxDocs dto);
        Task DeletePlanAsync(Guid id);
    }

    public class PlanosVoxDocsService : IPlanosVoxDocsService
    {
        private readonly IPlanosVoxDocsRepository _repository;
        private readonly PlanosValidator _validator;
        private readonly ILogger<PlanosVoxDocsService> _logger;

        public PlanosVoxDocsService(
            IPlanosVoxDocsRepository repository,
            PlanosValidator validator,
            ILogger<PlanosVoxDocsService> logger)
        {
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<PlanosVoxDocsModel> GetPlanByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Buscando plano por ID: {Id}", id);

                var plano = await _repository.ObterPorIdAsync(id) 
                    ?? throw new KeyNotFoundException($"Plano com ID {id} não encontrado");

                return plano;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Plano não encontrado");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar plano por ID");
                throw new ApplicationException("Ocorreu um erro ao buscar o plano", ex);
            }
        }

        public async Task<List<PlanosVoxDocsModel>> GetAllPlansAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os planos");

                var planos = await _repository.ObterTodosAsync();
                return planos.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os planos");
                throw new ApplicationException("Ocorreu um erro ao buscar os planos", ex);
            }
        }

        public async Task<PlanosVoxDocsModel> GetPlanByNameAsync(string name)
        {
            try
            {
                _logger.LogInformation("Buscando plano por nome: {Nome}", name);

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Nome do plano não pode ser vazio");

                var plano = await _repository.ObterPorNomeAsync(name);
                return plano;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar plano por nome");
                throw new ApplicationException("Ocorreu um erro ao buscar o plano", ex);
            }
        }

        public async Task<PlanosVoxDocsModel> GetPlanByNameAndPeriodicidadeAsync(string nome, string periodicidade)
        {
            try
            {
                _logger.LogInformation("Buscando plano por nome e periodicidade: {Nome}, {Periodicidade}", nome, periodicidade);

                var plano = await _repository.ObterPorNomeEPeriodicidadeAsync(nome, periodicidade);
                return plano;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar plano por nome e periodicidade");
                throw new ApplicationException("Ocorreu um erro ao buscar o plano", ex);
            }
        }

        public async Task<List<PlanosVoxDocsModel>> GetPlansByCategoryAsync(string categoria)
        {
            try
            {
                _logger.LogInformation("Buscando planos por categoria: {Categoria}", categoria);

                var planos = await _repository.ObterPorCategoriaAsync(categoria);
                return planos.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar planos por categoria");
                throw new ApplicationException("Ocorreu um erro ao buscar os planos", ex);
            }
        }

        public async Task<PlanosVoxDocsModel> CreatePlanAsync(DTOPlanosVoxDocs dto)
        {
            try
            {
                _logger.LogInformation("Criando novo plano: {Nome}", dto.Nome);

                // Converter DTO para Model
                var plano = new PlanosVoxDocsModel
                {
                    Id = Guid.NewGuid(),
                    Nome = dto.Nome?.Trim(),
                    Descriçao = dto.Descricao?.Trim(),
                    Preco = dto.Preco,
                    Periodicidade = dto.Periodicidade,
                    Duracao = dto.Duracao,
                    ArmazenamentoDisponivel = dto.ArmazenamentoDisponivel,
                    LimiteAdmin = dto.LimiteAdmin,
                    LimiteUsuario = dto.LimiteUsuario
                };

                // Validar usando o validator
                await _validator.ValidateAndThrowAsync(plano);

                // Verificar se plano já existe
                var planoExistente = await _repository.ObterPorNomeEPeriodicidadeAsync(plano.Nome, plano.Periodicidade);
                if (planoExistente != null)
                {
                    throw new InvalidOperationException($"Já existe um plano com nome {plano.Nome} e periodicidade {plano.Periodicidade}");
                }

                // Persistir
                var planoCriado = await _repository.AdicionarAsync(plano);

                _logger.LogInformation("Plano criado com sucesso. ID: {Id}", planoCriado.Id);

                return planoCriado;
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Erro de validação ao criar plano");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Regra de negócio violada");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar plano");
                throw new ApplicationException("Ocorreu um erro ao criar o plano", ex);
            }
        }

        public async Task<PlanosVoxDocsModel> UpdatePlanAsync(Guid id, DTOPlanosVoxDocs dto)
        {
            try
            {
                _logger.LogInformation("Atualizando plano ID: {Id}", id);

                // Obter plano existente
                var planoExistente = await _repository.ObterPorIdAsync(id);
                if (planoExistente == null)
                {
                    throw new KeyNotFoundException($"Plano com ID {id} não encontrado");
                }

                // Atualizar dados
                planoExistente.Nome = dto.Nome?.Trim();
                planoExistente.Descriçao = dto.Descricao?.Trim();
                planoExistente.Preco = dto.Preco;
                planoExistente.Periodicidade = dto.Periodicidade;
                planoExistente.Duracao = dto.Duracao;
                planoExistente.ArmazenamentoDisponivel = dto.ArmazenamentoDisponivel;
                planoExistente.LimiteAdmin = dto.LimiteAdmin;
                planoExistente.LimiteUsuario = dto.LimiteUsuario;

                // Validar usando o validator
                await _validator.ValidateAndThrowAsync(planoExistente);

                // Persistir
                await _repository.AtualizarAsync(planoExistente);

                _logger.LogInformation("Plano atualizado com sucesso. ID: {Id}", id);

                return planoExistente;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Plano não encontrado para atualização");
                throw;
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Erro de validação ao atualizar plano");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar plano");
                throw new ApplicationException("Ocorreu um erro ao atualizar o plano", ex);
            }
        }

        public async Task DeletePlanAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Removendo plano ID: {Id}", id);

                var plano = await _repository.ObterPorIdAsync(id);
                if (plano == null)
                {
                    throw new KeyNotFoundException($"Plano com ID {id} não encontrado");
                }

                await _repository.RemoverAsync(id);

                _logger.LogInformation("Plano removido com sucesso. ID: {Id}", id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Plano não encontrado para remoção");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover plano");
                throw new ApplicationException("Ocorreu um erro ao remover o plano", ex);
            }
        }
    }
}