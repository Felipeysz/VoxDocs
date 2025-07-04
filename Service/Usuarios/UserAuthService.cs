using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.Cookies;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public interface IUserAuthService
    {
        Task<ClaimsPrincipal> AuthenticateUserAsync(DTOLoginUsuario loginDto);
        Task<DTOUsuarioInfo> RegisterUserAsync(DTORegistrarUsuario registerDto);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task ResetPasswordWithTokenAsync(string token, string novaSenha);
        Task ChangePasswordAsync(string username, string senhaAntiga, string novaSenha);
        Task<ValidationResult> ValidateUserAsync(PagamentoUsuarioDTO userDto);
        Task<UserModel> GetUserByUsernameAsync(string username);
        Task<bool> AccountExistsAsync(string email, string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }

    public class UserAuthService : IUserAuthService
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<UserModel> _validator;
        private readonly IEmpresasContratanteRepository _empresaRepository;
        private readonly ILogger<UserAuthService> _logger;

        public UserAuthService(
            IUserRepository repository,
            IValidator<UserModel> validator,
            IEmpresasContratanteRepository empresaRepository,
            ILogger<UserAuthService> logger)
        {
            _repository = repository;
            _validator = validator;
            _empresaRepository = empresaRepository;
            _logger = logger;
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                _logger.LogInformation("Verificando existência de email: {Email}", email);
                return await _repository.ExistsAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Verificando existência de nome de usuário: {Username}", username);
                return await _repository.ExistsAsync(u => u.Usuario == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de nome de usuário: {Username}", username);
                throw;
            }
        }
        public async Task<bool> AccountExistsAsync(string email, string username)
        {
            try
            {
                _logger.LogInformation("Verificando existência de conta para Email: {Email}, Username: {Username}", email, username);

                // Verifica se existe algum usuário com o mesmo email OU username
                return await _repository.ExistsAsync(u =>
                    u.Email == email ||
                    u.Usuario == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de conta para Email: {Email}, Username: {Username}", email, username);
                throw;
            }
        }

        public async Task<ValidationResult> ValidateUserAsync(PagamentoUsuarioDTO userDto)
        {
            var userModel = new UserModel
            {
                Usuario = userDto.Usuario,
                Email = userDto.Email,
                Senha = userDto.Senha,
                PermissionAccount = userDto.PermissaoConta.ToString(),
                EmpresaContratante = userDto.EmpresaContratante
            };

            return await _validator.ValidateAsync(userModel);
        }

        public async Task<ClaimsPrincipal> AuthenticateUserAsync(DTOLoginUsuario loginDto)
        {
            try
            {
                _logger.LogInformation("Iniciando autenticação para: {Usuario}", loginDto.Usuario);

                var user = await _repository.GetByUsernameAsync(loginDto.Usuario);

                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado: {Usuario}", loginDto.Usuario);
                    throw new UnauthorizedAccessException("Credenciais inválidas ou conta inativa.");
                }

                _logger.LogDebug("Usuário encontrado. Ativo: {Ativo}", user.Ativo);

                // Verificação direta da senha (sem hash)
                bool isPasswordValid = loginDto.Senha == user.Senha;

                _logger.LogDebug("Resultado da verificação: {Resultado}", isPasswordValid);

                if (!user.Ativo || !isPasswordValid)
                {
                    _logger.LogWarning("Autenticação falhou. Ativo: {Ativo}, Senha válida: {SenhaValida}",
                        user.Ativo, isPasswordValid);
                    throw new UnauthorizedAccessException("Credenciais inválidas ou conta inativa.");
                }

                // Atualiza último login
                user.UltimoLogin = DateTime.UtcNow;
                await _repository.UpdateAsync(user);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.Usuario),
                    new(ClaimTypes.Email, user.Email),
                    new("PermissionAccount", user.PermissionAccount),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                _logger.LogInformation("Verificando a PermissionAccount do Usuario",
                user.Usuario, user.PermissionAccount);

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante autenticação para {Usuario}", loginDto.Usuario);
                throw;
            }
        }

        public async Task<DTOUsuarioInfo> RegisterUserAsync(DTORegistrarUsuario registerDto)
        {
            try
            {
                // 1. Validação básica do DTO
                if (registerDto == null)
                {
                    throw new ArgumentNullException(nameof(registerDto), "Os dados de registro não podem ser nulos");
                }

                // 2. Validação explícita da senha
                if (string.IsNullOrWhiteSpace(registerDto.Senha))
                {
                    throw new ArgumentException("A senha não pode ser nula ou vazia", nameof(registerDto.Senha));
                }

                _logger.LogInformation("Registrando novo usuário: {Usuario}", registerDto.Usuario);
                _logger.LogDebug("Dados recebidos - Email: {Email}, Senha: {SenhaLength} caracteres",
                    registerDto.Email, registerDto.Senha?.Length);

                // 3. Validação da empresa
                var empresaExiste = await _empresaRepository.ExistePorAsync(
                    e => e.EmpresaNome == registerDto.EmpresaContratante);

                if (!empresaExiste)
                {
                    throw new ValidationException("Empresa contratante não encontrada.");
                }

                // 4. Criação do modelo de usuário com validação adicional
                var userModel = new UserModel
                {
                    Id = Guid.NewGuid(),
                    Usuario = registerDto.Usuario ?? throw new ArgumentNullException(nameof(registerDto.Usuario)),
                    Email = registerDto.Email ?? throw new ArgumentNullException(nameof(registerDto.Email)),
                    Senha = registerDto.Senha, // Ainda não aplicamos hash aqui
                    PermissionAccount = registerDto.PermissaoConta ?? "User", // Valor padrão
                    PlanoPago = registerDto.PlanoPago,
                    EmpresaContratante = registerDto.EmpresaContratante
                        ?? throw new ArgumentNullException(nameof(registerDto.EmpresaContratante)),
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true,
                    LimiteUsuario = registerDto.LimiteUsuario ?? "0",
                    LimiteAdmin = registerDto.LimiteAdmin ?? "0"
                };

                // 5. Validação com FluentValidation
                var validationResult = await _validator.ValidateAsync(userModel);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("Falha na validação do usuário: {Errors}", errors);
                    throw new ValidationException(validationResult.Errors);
                }

                // 6. Log antes da criação (para debug)
                _logger.LogDebug("Criando usuário: {Usuario}, Email: {Email}",
                    userModel.Usuario, userModel.Email);

                // 7. Criação do usuário (o repositório aplicará o hash)
                var createdUser = await _repository.CreateAsync(userModel);

                _logger.LogInformation("Usuário {Usuario} registrado com sucesso (ID: {UserId})",
                    registerDto.Usuario, createdUser.Id);

                return ToUsuarioInfoDto(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário {Usuario}. Dados: {@RegisterDto}",
                    registerDto?.Usuario, registerDto);
                throw; // Re-lança a exceção para ser tratada no controller
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("Email não encontrado: {Email}", email);
                return null;
            }

            var token = Guid.NewGuid().ToString();
            await _repository.SetPasswordResetTokenAsync(user.Id, token, DateTime.UtcNow.AddHours(1));

            _logger.LogInformation("Token gerado para {Email}: {Token}", email, token);
            return token;
        }

        public async Task ResetPasswordWithTokenAsync(string token, string novaSenha)
        {
            try
            {
                _logger.LogInformation("Processando reset de senha com token: {Token}", token);

                // Validação da senha (mesmo sem hash, mantenha as regras)
                if (novaSenha.Length < 8 ||
                    !novaSenha.Any(char.IsUpper) ||
                    !novaSenha.Any(char.IsDigit) ||
                    !novaSenha.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    throw new ValidationException("A senha deve ter pelo menos 8 caracteres, incluindo uma letra maiúscula, um número e um caractere especial.");
                }

                var user = await _repository.GetByResetTokenAsync(token);
                if (user == null || user.PasswordResetTokenExpiration < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Token inválido ou expirado.");
                }

                // Armazena a senha em texto puro (NÃO SEGURO)
                user.Senha = novaSenha;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiration = null;

                await _repository.UpdateAsync(user);
                _logger.LogInformation("Senha resetada com sucesso para o usuário: {Usuario}", user.Usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao resetar senha com token");
                throw;
            }
        }

        public async Task ChangePasswordAsync(string username, string senhaAntiga, string novaSenha)
        {
            try
            {
                _logger.LogInformation("Solicitada alteração de senha para: {Usuario}", username);

                // Validação da nova senha
                if (novaSenha.Length < 8 ||
                    !novaSenha.Any(char.IsUpper) ||
                    !novaSenha.Any(char.IsDigit) ||
                    !novaSenha.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    throw new ValidationException("A senha deve ter pelo menos 8 caracteres, incluindo uma letra maiúscula, um número e um caractere especial.");
                }

                var user = await _repository.GetByUsernameAsync(username);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Credenciais inválidas.");
                }

                // Verifica senha antiga (comparação direta)
                if (senhaAntiga != user.Senha)
                {
                    throw new UnauthorizedAccessException("Credenciais inválidas.");
                }

                // Armazena a nova senha em texto puro (NÃO SEGURO)
                user.Senha = novaSenha;
                await _repository.UpdateAsync(user);

                _logger.LogInformation("Senha alterada com sucesso para: {Usuario}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha para {Usuario}", username);
                throw;
            }
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Getting user by username: {Username}", username);
                var user = await _repository.GetByUsernameAsync(username);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        private static DTOUsuarioInfo ToUsuarioInfoDto(UserModel user)
        {
            if (user == null) return null;

            return new DTOUsuarioInfo
            {
                Id = user.Id,
                Usuario = user.Usuario,
                Email = user.Email,
                PermissaoConta = user.PermissionAccount,
                EmpresaContratante = user.EmpresaContratante,
                Plano = user.PlanoPago,
                DataCriacao = user.DataCriacao,
                UltimoLogin = user.UltimoLogin,
                Ativo = user.Ativo
            };
        }
    }
}