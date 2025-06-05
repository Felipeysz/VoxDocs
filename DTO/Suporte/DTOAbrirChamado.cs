using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO
{
    public class DTOAbrirChamado
    {
        [Required]
        public string Assunto { get; set; }

        [Required]
        public string Descricao { get; set; }

        // Id do usuário que está abrindo o chamado
        [Required]
        public int UsuarioId { get; set; }

        // Nome do usuário que abre (para exibição rápida; pode ser obtido via UserService, mas armazenamos aqui para persistir)
        [Required]
        public string NomeUsuario { get; set; }
    }
}
