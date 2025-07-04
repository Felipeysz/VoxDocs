using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO;

public class DTOEmpresaCreate
{
    public Guid Id { get; set; }
    [StringLength(100, ErrorMessage = "O nome da empresa não pode exceder 100 caracteres")]
    public required string EmpresaNome { get; set; }

    [EmailAddress(ErrorMessage = "Informe um endereço de email válido")]
    public required string EmailEmpresa { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public required Guid PlanoId { get; set; }
}