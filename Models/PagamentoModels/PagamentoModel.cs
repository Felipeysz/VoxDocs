using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    public class PagamentoAtivoModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        public Guid PlanoId { get; set; }

        [Required]
        public string EmpresaNome { get; set; }

        [Required]
        public string NomePlanoPago { get; set; }

        [Required]
        public string MetodoPagamento { get; set; }

        [Required]
        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DataExpiracao { get; set; }

        [Required]
        public decimal ValorPago { get; set; }

        [Required]
        public StatusPlano Status { get; set; }

        // ✅ Nova propriedade para armazenar múltiplas pastas principais
        public List<Guid> PastaPrincipalIds { get; set; } = new List<Guid>();
        public List<string> UsuariosCriados { get; set; } = new List<string>();
    }
    
    public enum StatusPlano
    {
        Ativo,
        Inativo,
        Cancelado
    }
}