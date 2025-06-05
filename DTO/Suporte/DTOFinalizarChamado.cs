using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO
{
    public class DTOFinalizarChamado
    {
        [Required]
        public int ChamadoId { get; set; }

        [Required]
        public int SuporteResponsavelId { get; set; }
        public string MensagemEncerramento { get; set; }
        public string NomeSuporteResponsavel { get; internal set; }
    }
}
