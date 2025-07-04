using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    [Table("SubPastas")]
    public class SubPastaModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O nome da subpasta é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome da subpasta deve ter entre 3 e 100 caracteres")]
        public required string NomeSubPasta { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Relação com Pasta Principal
        [ForeignKey("PastaPrincipal")]
        public Guid PastaPrincipalId { get; set; }
        public PastaPrincipalModel PastaPrincipal { get; set; }
    }
}