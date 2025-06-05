using System;
using System.Collections.Generic;

namespace VoxDocs.Models
{
    public enum StatusChamado
    {
        Aberto,
        EmAtendimento,
        Resolvido,
        Fechado,
        Arquivado
    }

    public class ChamadoModel
    {
        public int Id { get; set; }

        public string Assunto { get; set; }

        public string Descricao { get; set; }

        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }

        public int? SuporteResponsavelId { get; set; }

        public string NomeSuporteResponsavel { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public StatusChamado Status { get; set; }

        public ICollection<MensagemModel> Mensagens { get; set; } = new List<MensagemModel>();
    }
}
