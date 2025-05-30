namespace VoxDocs.DTO
{
    // CARTÃO: cliente envia só a identificação do plano + dados do cartão
    public class DTOCartaoPagamentoFalso
    {
        public required string CartaoNumber { get; set; }
        public required string ValidadeCartao { get; set; }
        public required string CvvCartao { get; set; }
        public required string TipoCartao { get; set; }    // "Credito" ou "Debito"
        public required string TipoPlano { get; set; }     // ex: "Plano Básico Mensal"
    }

    // PIX: etapa 1 — cliente solicita QRCode fornecendo apenas o plano
    public class DTOPixGerar
    {
        public required string TipoPlano { get; set; }
    }

    // PIX: etapa 2 — cliente confirma o pagamento
    public class DTOPixConfirmar
    {
        public required string Token { get; set; } // Agora confirmamos via token
    }
}
