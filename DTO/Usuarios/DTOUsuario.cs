using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VoxDocs.DTO
{
    // DTO para registro de usuário
    public class DTORegistrarUsuario
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        public required string Senha { get; set; }

        [Required(ErrorMessage = "Account permission is required")]
        public required string PermissaoConta { get; set; } // "admin" ou "user"

        [Required(ErrorMessage = "Contracting company is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public required string EmpresaContratante { get; set; }

        [StringLength(50, ErrorMessage = "Paid plan cannot exceed 50 characters")]
        public string? PlanoPago { get; set; } // "gratuito", "premium", etc.
        public string? LimiteUsuario { get; set; }
        public string? LimiteAdmin { get; set; }
    }

    // DTO para login de usuário
    public class DTOLoginUsuario
    {
        [Required(ErrorMessage = "Username or email is required")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Senha { get; set; }
    }

    // DTO para atualização de usuário
    public class DTOAtualizarUsuario
    {
        [Required]
        public required Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Account permission is required")]
        public required string PermissaoConta { get; set; }

        [Required(ErrorMessage = "Contracting company is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public required string EmpresaContratante { get; set; }

        [StringLength(50, ErrorMessage = "Paid plan cannot exceed 50 characters")]
        public string? PlanoPago { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Must be a numeric value")]
        public string? LimiteAdmin { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Must be a numeric value")]
        public string? LimiteUsuario { get; set; }

        [Required]
        public bool Ativo { get; set; }
    }

    // DTO para informações básicas do usuário (resposta)
    public class DTOUsuarioInfo
    {
        public Guid Id { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string PermissaoConta { get; set; }
        public string EmpresaContratante { get; set; }
        public string? Plano { get; set; }
        public string? LimiteAdmin { get; set; }
        public string? LimiteUsuario { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Ativo { get; set; }
    }

    // DTO para informações de armazenamento do usuário
    public class DTOArmazenamentoUsuario
    {
        public decimal UsoArmazenamento { get; set; } // Em MB/GB
        public decimal LimiteArmazenamento { get; set; } // Em MB/GB
    }

    // DTO para resposta de registro (opcional)
    public class DTORegistroResponse
    {
        public DTOUsuarioInfo Usuario { get; set; }
        public string? LimiteAdmin { get; set; }
        public string? LimiteUsuario { get; set; }
    }

    [JsonDerivedType(typeof(PagamentoUsuarioDTO))]
    public class PagamentoUsuarioDTO
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Account permission is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoPermissao PermissaoConta { get; set; } = TipoPermissao.User;

        public string EmpresaContratante { get; set; }

        // Outras propriedades serializáveis
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
    }

    public enum TipoPermissao
    {
        Admin,
        User
    }

    public class UserInfoDTO
    {
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string NivelPermissao { get; set; }
    }

}