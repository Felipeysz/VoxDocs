using System;

namespace VoxDocs.Models
{
    public class PagamentoCartaoFalsoModel
    {
        public int Id { get; set; }
        public string CartaoNumber { get; set; }
        public string ValidadeCartao { get; set; }
        public string CvvCartao { get; set; }
        public string TipoCartao { get; set; }
        public string TipoPlano { get; set; }
        public string EmpresaContratante { get; internal set; }
    }

    public class PagamentoPixModel
    {
        public int Id { get; set; }
        public string QRCodePix { get; set; }
        public string TipoPlano { get; set; }
        public DateTime DataCriacao { get; set; } // Nova propriedade para gerenciar tempo
        public string EmpresaContratante { get; internal set; }
        public DateTime DataExpiracao { get; internal set; }
    }
    
    public class PagamentoConcluido
    {
        public int Id { get; set; }
        public string EmpresaContratante { get; set; }
        public string MetodoPagamento { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataExpiracao { get; set; }
        public string StatusEmpresa { get; set; } = "Plano Ativo";
    }
}