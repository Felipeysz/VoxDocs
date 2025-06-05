namespace VoxDocs.DTO
{
    public class PagamentoCartaoRequestDto
    {
        public string TipoPlano { get; set; } = string.Empty;
        public string CartaoNumber { get; set; } = string.Empty;
        public string ValidadeCartao { get; set; } = string.Empty;
        public string CvvCartao { get; set; } = string.Empty;
        public string TipoCartao { get; set; } = string.Empty;
        public string EmpresaContratante { get; set; } = string.Empty;
    }

    public class PagamentoPixRequestDto
    {
        public string TipoPlano { get; set; } = string.Empty;
        public string EmpresaContratante { get; set; } = string.Empty;
    
    }

    // Nova DTO para PagamentoConcluido
    public class PagamentoConcluidoDto
    {
        public int Id { get; set; }
        public string EmpresaContratante { get; set; }
        public string MetodoPagamento { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataExpiracao { get; set; }
        public string StatusEmpresa { get; set; }
    }

    // DTO para criação de PagamentoConcluido
    public class PagamentoConcluidoCreateDto
    {
        public string EmpresaContratante { get; set; }
        public string MetodoPagamento { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataExpiracao { get; set; }
    }
}