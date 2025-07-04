using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    [Table("PlanosVoxDocs")]
    public class PlanosVoxDocsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [StringLength(500)]
        [Column("Descricao")] // Fixing the special character in property name
        public required string Descriçao { get; set; }

        [Required]
        [Range(0, 999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal Preco { get; set; }

        [Required]
        [StringLength(20)]
        public required string Periodicidade { get; set; } // Mensal, Trimestral, Semestral

        [Range(1, 36)]
        public required int? Duracao { get; set; }

        [Range(1, 9999)]
        public required int? ArmazenamentoDisponivel { get; set; }

        [Range(1, 100)]
        public required int? LimiteAdmin { get; set; }

        [Range(1, 1000)]
        public required int? LimiteUsuario { get; set; }
    }
}