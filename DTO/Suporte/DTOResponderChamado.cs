using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO
{
    public class DTOResponderChamado
    {
        [Required]
        public int ChamadoId { get; set; }

        [Required]
        public int SuporteResponsavelId { get; set; }

        // Nome do suporte que está respondendo
        [Required]
        public string NomeSuporteResponsavel { get; set; }

        [Required]
        public string Mensagem { get; set; }
    }
}
