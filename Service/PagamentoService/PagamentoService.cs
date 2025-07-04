using FluentValidation;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.Validators;
using VoxDocs.Repositories;
using Microsoft.EntityFrameworkCore;
using StatusPlano = VoxDocs.Models.StatusPlano;
using FluentValidation.Results;

namespace VoxDocs.Services
{
    public interface IPagamentoService
    {
        Task<PlanoSelecionadoDTO> SelecionarPlanoAsync(Guid planoId);
        Task<PagamentoConcluidoDTO> PagamentoConcluidoPlanoAsync(PagamentoDTO dto);
        Task LimparPagamentosExpiradosAsync();
        Task<bool> EmpresaTemPagamentoAtivoAsync(string empresaNome);
        Task<bool> VerificarEmpresaExistenteAsync(string nomeEmpresa, string emailEmpresa);
        Task<ValidationResult> ValidateUserAsync(PagamentoUsuarioDTO userDto);
    }

    public class PagamentoService : IPagamentoService
    {
        private readonly IPagamentoAtivoRepository _pagamentoRepository;
        private readonly IEmpresasContratanteService _empresaService;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly ILogger<PagamentoService> _logger;
        private readonly IValidator<PagamentoDTO> _pagamentoDtoValidator;
        private readonly IPagamentoValidator _pagamentoAtivoValidator;
        private readonly IPastaPrincipalService _pastaPrincipalService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserAuthService _userAuthService;

        public PagamentoService(
            IPagamentoAtivoRepository pagamentoRepository,
            IEmpresasContratanteService empresaService,
            IPlanosVoxDocsService planosService,
            ILogger<PagamentoService> logger,
            IValidator<PagamentoDTO> pagamentoDtoValidator,
            IPagamentoValidator pagamentoAtivoValidator,
            IPastaPrincipalService pastaPrincipalService,
            ISubPastaService subPastaService,
            IUserAuthService userAuthService)
        {
            _pagamentoRepository = pagamentoRepository;
            _empresaService = empresaService;
            _planosService = planosService;
            _logger = logger;
            _pagamentoDtoValidator = pagamentoDtoValidator;
            _pagamentoAtivoValidator = pagamentoAtivoValidator;
            _pastaPrincipalService = pastaPrincipalService;
            _subPastaService = subPastaService;
            _userAuthService = userAuthService;
        }

        public async Task<ValidationResult> ValidateUserAsync(PagamentoUsuarioDTO userDto)
        {
            return await _userAuthService.ValidateUserAsync(userDto);
        }

        public async Task<PlanoSelecionadoDTO> SelecionarPlanoAsync(Guid planoId)
        {
            try
            {
                _logger.LogInformation("Selecionando plano. PlanoId: {PlanoId}", planoId);

                if (planoId == Guid.Empty)
                    throw new ArgumentException("ID do plano não pode ser vazio");

                var plano = await _planosService.GetPlanByIdAsync(planoId) ??
                    throw new KeyNotFoundException($"Plano com ID {planoId} não encontrado");

                return new PlanoSelecionadoDTO
                {
                    Id = plano.Id,
                    Nome = plano.Nome,
                    Descricao = plano.Descriçao,
                    Preco = plano.Preco,
                    Periodicidade = plano.Periodicidade,
                    Duracao = plano.Duracao,
                    Armazenamento = plano.ArmazenamentoDisponivel,
                    LimiteAdmin = plano.LimiteAdmin,
                    LimiteUsuario = plano.LimiteUsuario,
                    DataValidade = DateTime.UtcNow.AddMonths(plano.Duracao ?? 1),
                    Caracteristicas = ObterCaracteristicasPlano(plano)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao selecionar plano");
                throw;
            }
        }

        public async Task<PagamentoConcluidoDTO> PagamentoConcluidoPlanoAsync(PagamentoDTO dto)
        {
            var executionStrategy = _pagamentoRepository.GetExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                // Validação do DTO
                var validationResult = await _pagamentoDtoValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                using var transaction = await _pagamentoRepository.BeginTransactionAsync();

                try
                {
                    // 1. Obter e validar plano
                    var plano = await ValidarEObterPlanoAsync(dto.PlanoId);

                    // 2. Criar empresa com validação do ID retornado
                    var empresaId = await CriarEmpresaContratanteAsync(dto, plano);
                    if (empresaId == Guid.Empty)
                    {
                        throw new InvalidOperationException("Falha ao criar empresa - ID inválido retornado");
                    }

                    // Dicionário para mapear nomes de pastas principais para seus IDs
                    var pastasPrincipaisCriadas = new Dictionary<string, Guid>();

                    // 3. Processar pastas principais (se existirem no DTO)
                    if (dto.PastasPrincipais != null && dto.PastasPrincipais.Any())
                    {
                        foreach (var pastaModel in dto.PastasPrincipais)
                        {
                            var pastaDto = new PastaPrincipalCreateDTO
                            {
                                NomePastaPrincipal = pastaModel.Nome
                            };

                            // Adicionar log antes de criar
                            _logger.LogInformation("Criando pasta principal '{PastaNome}' para empresa {EmpresaId} (fluxo de pagamento)",
                                pastaDto.NomePastaPrincipal, empresaId);

                            var pastaCriada = await _pastaPrincipalService.CriarPastaPrincipalAsync(
                                pastaDto,
                                empresaId); // ← Garantir que está true

                            pastasPrincipaisCriadas[pastaModel.Nome] = pastaCriada.Id;
                        }
                    }

                    // 4. Processar subpastas (se existirem no DTO)
                    if (dto.SubPastas != null && dto.SubPastas.Any())
                    {
                        foreach (var subPastaModel in dto.SubPastas)
                        {
                            // Verificar se a pasta principal existe
                            if (!pastasPrincipaisCriadas.TryGetValue(subPastaModel.PastaPrincipalNome, out var pastaPrincipalId))
                            {
                                throw new KeyNotFoundException($"Pasta principal '{subPastaModel.PastaPrincipalNome}' não encontrada para criar subpasta");
                            }

                            var subPastaDto = new SubPastaCreateDTO
                            {
                                PastaPrincipalId = pastaPrincipalId,
                                NomeSubPasta = subPastaModel.Nome
                            };
                            await _subPastaService.CriarSubPastaAsync(subPastaDto);
                        }
                    }

                    // 5. Processar usuários (se existirem no DTO)
                    if (dto.Usuarios != null && dto.Usuarios.Any())
                    {
                        await ValidarLimitesUsuariosAsync(dto.Usuarios, plano);

                        foreach (var usuarioDto in dto.Usuarios)
                        {
                            // Valida o usuário antes de criar
                            var validation = await ValidateUserAsync(usuarioDto);
                            if (!validation.IsValid)
                            {
                                throw new ValidationException(validation.Errors);
                            }

                            var registroDto = new DTORegistrarUsuario
                            {
                                Usuario = usuarioDto.Usuario,
                                Email = usuarioDto.Email,
                                Senha = usuarioDto.Senha, // Senha em texto puro
                                PermissaoConta = usuarioDto.PermissaoConta.ToString(),
                                EmpresaContratante = dto.EmpresaNome,
                                PlanoPago = plano.Nome,
                                LimiteUsuario = plano.LimiteUsuario.ToString(),
                                LimiteAdmin = plano.LimiteAdmin.ToString()
                            };
                            await _userAuthService.RegisterUserAsync(registroDto);
                        }
                    }
                    else
                    {
                        // Criar usuário admin padrão se nenhum usuário foi fornecido
                        var usuarioAdminDto = new DTORegistrarUsuario
                        {
                            Usuario = "UsuarioAdmin",
                            Email = dto.EmailEmpresa,
                            Senha = "senhaAdmin123", // Senha padrão em texto puro
                            PermissaoConta = TipoPermissao.Admin.ToString(),
                            EmpresaContratante = dto.EmpresaNome,
                            PlanoPago = plano.Nome,
                            LimiteUsuario = plano.LimiteUsuario.ToString(),
                            LimiteAdmin = plano.LimiteAdmin.ToString()
                        };
                        await _userAuthService.RegisterUserAsync(usuarioAdminDto);
                        _logger.LogInformation("Usuário admin padrão criado");
                    }

                    // 6. Registrar pagamento
                    var pagamento = new PagamentoAtivoModel
                    {
                        EmpresaId = empresaId,
                        PlanoId = dto.PlanoId,
                        EmpresaNome = dto.EmpresaNome,
                        NomePlanoPago = plano.Nome,
                        MetodoPagamento = dto.MetodoPagamento,
                        DataPagamento = DateTime.UtcNow,
                        DataExpiracao = DateTime.UtcNow.AddMonths(plano.Duracao ?? 1),
                        ValorPago = plano.Preco,
                        Status = StatusPlano.Ativo,
                        PastaPrincipalIds = pastasPrincipaisCriadas.Values.ToList(),
                        UsuariosCriados = dto.Usuarios?.Select(u => u.Email).ToList() ?? new List<string>()
                    };

                    var pagamentoValidation = _pagamentoAtivoValidator.ValidateRegistroNovoPagamento(pagamento);
                    if (!pagamentoValidation.IsValid)
                    {
                        throw new FluentValidation.ValidationException(pagamentoValidation.Errors);
                    }

                    var pagamentoRegistrado = await _pagamentoRepository.AdicionarAsync(pagamento);
                    await transaction.CommitAsync();

                    return new PagamentoConcluidoDTO
                    {
                        Sucesso = true,
                        EmpresaId = empresaId,
                        PlanoId = plano.Id,
                        PagamentoId = pagamentoRegistrado.Id,
                        DataExpiracao = pagamentoRegistrado.DataExpiracao,
                        Status = (DTO.StatusPlano)pagamentoRegistrado.Status,
                        LimiteAdmin = plano.LimiteAdmin,
                        LimiteUsuario = plano.LimiteUsuario,
                        PastaPrincipalIds = pagamentoRegistrado.PastaPrincipalIds,
                        UsuariosCriados = dto.Usuarios?.Select(u => new DTOUsuarioInfo
                        {
                            Usuario = u.Usuario,
                            Email = u.Email,
                            PermissaoConta = u.PermissaoConta.ToString()
                        }).ToList() ?? new List<DTOUsuarioInfo>()
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro no processo de pagamento");
                    throw;
                }
            });
        }

        public async Task<bool> EmpresaTemPagamentoAtivoAsync(string empresaNome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empresaNome))
                    throw new ArgumentException("O nome da empresa não pode ser vazio.");

                var pagamento = await _pagamentoRepository.ObterPorEmpresaAsync(empresaNome);
                if (pagamento == null) return false;

                var validation = _pagamentoAtivoValidator.ValidatePagamentoAtivo(pagamento);
                return validation.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar pagamento ativo");
                return false;
            }
        }

        public async Task LimparPagamentosExpiradosAsync()
        {
            try
            {
                var pagamentosExpirados = (await _pagamentoRepository.ObterTodosAsync())
                    .Where(p => p.DataExpiracao < DateTime.UtcNow)
                    .ToList();

                foreach (var pagamento in pagamentosExpirados)
                {
                    try
                    {
                        await _pagamentoRepository.RemoverAsync(pagamento.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao remover pagamento expirado ID {pagamento.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na limpeza de pagamentos expirados");
                throw;
            }
        }

        public async Task<bool> VerificarEmpresaExistenteAsync(string nomeEmpresa, string emailEmpresa)
        {
            try
            {
                return await _empresaService.VerificarEmpresaExistenteAsync(nomeEmpresa, emailEmpresa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de empresa");
                throw;
            }
        }

        private async Task<PlanosVoxDocsModel> ValidarEObterPlanoAsync(Guid planoId)
        {
            var plano = await _planosService.GetPlanByIdAsync(planoId);
            if (plano == null)
            {
                throw new KeyNotFoundException($"Plano com ID {planoId} não encontrado");
            }
            return plano;
        }

        private async Task<Guid> CriarEmpresaContratanteAsync(PagamentoDTO dto, PlanosVoxDocsModel plano)
        {
            var empresaDto = new DTOEmpresaCreate
            {
                EmpresaNome = dto.EmpresaNome,
                EmailEmpresa = dto.EmailEmpresa,
                PlanoId = plano.Id,
                DataExpiracao = DateTime.UtcNow.AddMonths(plano.Duracao ?? 1)
            };

            return await _empresaService.CriarEmpresaAsync(empresaDto);
        }

        private string[] ObterCaracteristicasPlano(PlanosVoxDocsModel plano)
        {
            return new string[]
            {
                $"Armazenamento: {plano.ArmazenamentoDisponivel} GB",
                $"Administradores: {plano.LimiteAdmin}",
                $"Usuários: {plano.LimiteUsuario}",
                $"Periodicidade: {plano.Periodicidade}",
                $"Duração: {plano.Duracao} meses"
            };
        }

        private async Task ValidarLimitesUsuariosAsync(List<PagamentoUsuarioDTO> usuarios, PlanosVoxDocsModel plano)
        {
            var adminCount = usuarios.Count(u => u.PermissaoConta == TipoPermissao.Admin);
            var userCount = usuarios.Count(u => u.PermissaoConta == TipoPermissao.User);

            if (adminCount > plano.LimiteAdmin)
                throw new InvalidOperationException($"O plano permite no máximo {plano.LimiteAdmin} administradores.");

            if (userCount > plano.LimiteUsuario)
                throw new InvalidOperationException($"O plano permite no máximo {plano.LimiteUsuario} usuários comuns.");
        }
    }
}