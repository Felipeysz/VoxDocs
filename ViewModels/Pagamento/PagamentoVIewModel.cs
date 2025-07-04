using System.ComponentModel.DataAnnotations;
using VoxDocs.DTO;

namespace VoxDocs.PagamentoViewModel
{
    public class CadastroEmpresaViewModel
    {
        [Required(ErrorMessage = "Nome da empresa é obrigatório")]
        [StringLength(100, MinimumLength = 4)]
        public string EmpresaNome { get; set; }

        [Required(ErrorMessage = "E-mail corporativo é obrigatório")]
        [EmailAddress]
        [StringLength(100)]
        public string EmailEmpresa { get; set; }

        public Guid PlanoId { get; set; }
        public string NomePlano { get; set; }
    }

    public class PastaPrincipalViewModel
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }
    }

    public class SubPastaViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "O nome da subpasta é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "A pasta principal é obrigatória")]
        public Guid PastaPrincipalId { get; set; }
        
        // Adicione esta propriedade para exibição
        public string PastaPrincipalNome { get; set; }
    }

    public class PastasViewModel
    {
        public Guid? PastaPrincipalId { get; set; }
        public List<PastaPrincipalViewModel> PastasPrincipais { get; set; } = new();
        public List<SubPastaViewModel> SubPastas { get; set; } = new();
    }

    public class CadastroUsuariosViewModel
    {
        public List<PagamentoUsuarioDTO> Usuarios { get; set; } = new();
        public int LimiteAdmin { get; set; }
        public int LimiteUsuario { get; set; }
        public PagamentoUsuarioDTO NovoUsuario { get; set; } = new();
    }

    public class FinalizarPagamentoViewModel
    {
        public Guid PlanoId { get; set; }
        public string NomePlano { get; set; }
        public decimal ValorPago { get; set; }
        public string EmpresaNome { get; set; }
        public string EmailEmpresa { get; set; }
        
        [Required]
        public string MetodoPagamento { get; set; }
        
        public List<string> Pastas { get; set; } = new();
        public List<string> SubPastas { get; set; } = new(); // Adicione esta linha
        public List<PagamentoUsuarioDTO> Usuarios { get; set; } = new();
    }
}