using System;

namespace VoxDocs.Models
{
    public class MensagemModel
    {
        public int Id { get; set; }
        public int ChamadoId { get; set; }
        public ChamadoModel Chamado { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool EnviadoPorSuporte { get; set; }
        public string NomeRemetente { get; set; }
    }
}
