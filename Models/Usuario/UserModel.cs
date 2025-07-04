using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [Display(Name = "Username")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Senha { get; set; }

        [Required(ErrorMessage = "Account permission level is required")]
        [Display(Name = "Account Permission")]
        public required string PermissionAccount { get; set; }

        [Required(ErrorMessage = "Contracting company is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        [Display(Name = "Contracting Company")]
        public required string EmpresaContratante { get; set; }

        [StringLength(50, ErrorMessage = "Paid plan cannot exceed 50 characters")]
        [Display(Name = "Paid Plan")]
        public string? PlanoPago { get; set; }

        [Display(Name = "User Limit")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Must be a numeric value")]
        public string? LimiteUsuario { get; set; } = "0";

        [Display(Name = "Admin Limit")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Must be a numeric value")]
        public string? LimiteAdmin { get; set; } = "0";

        [Display(Name = "Password Reset Token")]
        public string? PasswordResetToken { get; set; }

        [Display(Name = "Password Reset Token Expiration")]
        public DateTime? PasswordResetTokenExpiration { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Creation Date")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Login")]
        public DateTime? UltimoLogin { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool Ativo { get; set; } = true;
    }
}