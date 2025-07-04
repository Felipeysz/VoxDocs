namespace VoxDocs.Models
{
    public class SuporteDetailsViewModel
    {
        public int Id { get; set; }
        public string CriadoPor { get; set; }
        public string Assunto { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public string Responsavel { get; set; }
        public List<MensagemSuporte> Mensagens { get; set; }
    }

    public class MensagemSuporte
    {
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public string Origem { get; set; }
        public string Remetente { get; set; }
    }

    public class SuporteListViewModel
    {
        public List<SuporteDetailsViewModel> ChamadosAbertos { get; set; }
        public List<SuporteDetailsViewModel> ChamadosFechados { get; set; }
    }
}