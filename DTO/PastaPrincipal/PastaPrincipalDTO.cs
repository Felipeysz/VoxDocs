using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO
{
    // DTO específico para criação (sem ID)
    public class PastaPrincipalCreateDTO
    {
        public Guid Id { get; set; }  // Adicione esta linha
        [Required(ErrorMessage = "O nome da pasta principal é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da pasta principal não pode exceder 100 caracteres.")]
        public required string NomePastaPrincipal { get; set; }

        [Required(ErrorMessage = "O token de acesso é obrigatório.")]
        [StringLength(100, ErrorMessage = "O token de acesso não pode exceder 100 caracteres.")]

        public Guid EmpresaId { get; set; }
        public DateTime? DataCriacao { get; set; }
    }

    public class PastaPrincipalDTO
    {
        public Guid Id { get; set; }
        public string NomePastaPrincipal { get; set; }
        public DateTime DataCriacao { get; set; }
        public Guid EmpresaId { get; set; }
    }

    // DTO opcional para exibição/consulta
    public class PastaPrincipalResponseDTO
    {
        public Guid Id { get; set; }
        public string NomePastaPrincipal { get; set; }
        public string TokenAcesso { get; set; }
        public DateTime DataCriacao { get; set; }
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } // Opcional - pode vir do relacionamento
    }
}