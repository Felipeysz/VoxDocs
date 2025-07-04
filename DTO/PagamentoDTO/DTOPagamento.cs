using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VoxDocs.PagamentoViewModel;

namespace VoxDocs.DTO
{
    public class PagamentoDTO
    {
        // Informações da empresa
        [Required(ErrorMessage = "Nome da empresa é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome da empresa não pode exceder 100 caracteres")]
        public string EmpresaNome { get; set; }

        [Required(ErrorMessage = "Email da empresa é obrigatório")]
        [EmailAddress(ErrorMessage = "Email da empresa inválido")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
        public string EmailEmpresa { get; set; }

        // Informações do pagamento
        [Required(ErrorMessage = "Método de pagamento é obrigatório")]
        [StringLength(50, ErrorMessage = "Método de pagamento não pode exceder 50 caracteres")]
        public string MetodoPagamento { get; set; }

        [Required(ErrorMessage = "ID do plano é obrigatório")]
        public Guid PlanoId { get; set; }

        // Estrutura de pastas
        public List<PastaPrincipalViewModel> PastasPrincipais { get; set; } = new();
         public List<SubPastaViewModel> SubPastas { get; set; } = new();

        // Usuários a serem criados
        public List<PagamentoUsuarioDTO> Usuarios { get; set; } = new List<PagamentoUsuarioDTO>();

        // Identificador da empresa (opcional para criação)
        public Guid? EmpresaId { get; set; }
    }

    public class PagamentoConcluidoDTO
    {
        public Guid PagamentoId { get; set; }
        public bool Sucesso { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid PlanoId { get; set; }
        public DateTime DataExpiracao { get; set; }
        public StatusPlano Status { get; set; }
        public int? LimiteAdmin { get; set; }
        public int? LimiteUsuario { get; set; }
        public List<Guid> PastaPrincipalIds { get; set; } = new List<Guid>();
        public List<DTOUsuarioInfo> UsuariosCriados { get; set; } = new List<DTOUsuarioInfo>();
    }
}