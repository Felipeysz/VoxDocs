namespace VoxDocs.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Informações do Usuário
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string EmpresaContratante { get; set; }
        public string PermissaoConta { get; set; }

        // Informações do Plano
        public string Plano { get; set; }
        public string ArmazenamentoTotal { get; set; }
        public string ArmazenamentoUsado { get; set; }
        public double PercentualUsoArmazenamento { get; set; }

        // Documentos
        public int DocumentosEnviados { get; set; }

        // Pagamento
        public string StatusPagamento { get; set; }
        public string DataProximoPagamento { get; set; }

        // Timestamp
        public string UltimaAtualizacao { get; set; }
    }
}