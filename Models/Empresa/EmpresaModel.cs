using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    [Table("EmpresasContratante")]
    public class EmpresasContratanteModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Plano")]
        public Guid PlanoId { get; set; }
        public PlanosVoxDocsModel Plano { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da empresa não pode exceder 100 caracteres.")]
        public required string EmpresaNome { get; set; }

        [EmailAddress(ErrorMessage = "O email da empresa não está em um formato válido.")]
        [StringLength(100, ErrorMessage = "O email da empresa não pode exceder 100 caracteres.")]
        public string? EmailEmpresa { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataExpiracao { get; set; }
        public bool PlanoAtivo { get; set; } = true;

        // Relação 1:N com PastasPrincipais
        public ICollection<PastaPrincipalModel> PastasPrincipais { get; set; } = new List<PastaPrincipalModel>();
    }
}