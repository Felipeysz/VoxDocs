using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    [Table("PastasPrincipais")]
    public class PastaPrincipalModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O nome da pasta principal é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da pasta principal não pode exceder 100 caracteres.")]
        public required string NomePastaPrincipal { get; set; }

        [Required(ErrorMessage = "O token de acesso é obrigatório.")]
        [StringLength(100, ErrorMessage = "O token de acesso não pode exceder 100 caracteres.")]

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [ForeignKey("Empresa")]
        public Guid EmpresaId { get; set; }
        public EmpresasContratanteModel Empresa { get; set; }
        public ICollection<SubPastaModel> SubPastas { get; set; } = new List<SubPastaModel>();
    }
}