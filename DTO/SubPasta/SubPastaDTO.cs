using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VoxDocs.DTO
{
    public class SubPastaCreateDTO
    {
        public Guid Id { get; set; }  // Adicione esta linha
        [Required(ErrorMessage = "O ID da pasta principal é obrigatório")]
        [JsonPropertyName("pastaPrincipalId")]
        public required Guid PastaPrincipalId { get; set; }

        [Required(ErrorMessage = "O nome da subpasta é obrigatório")]
        [StringLength(100, MinimumLength = 3, 
            ErrorMessage = "O nome da subpasta deve ter entre 3 e 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\u00C0-\u00FF \-_]+$", 
            ErrorMessage = "O nome contém caracteres inválidos")]
        [JsonPropertyName("nomeSubPasta")]
        public required string NomeSubPasta { get; set; }
    }

    public class SubPastaUpdateDTO
    {
        [Required(ErrorMessage = "O ID da subpasta é obrigatório")]
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        [Required(ErrorMessage = "O nome da subpasta é obrigatório")]
        [StringLength(100, MinimumLength = 3, 
            ErrorMessage = "O nome da subpasta deve ter entre 3 e 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\u00C0-\u00FF \-_]+$", 
            ErrorMessage = "O nome contém caracteres inválidos")]
        [JsonPropertyName("nomeSubPasta")]
        public required string NomeSubPasta { get; set; }
    }
}