using System.Collections.Generic;
using VoxDocs.Models;

namespace VoxDocs.ViewModels
{

    public class PagamentosViewModel
    {
        public string Categoria { get; set; }
        public List<PlanosVoxDocsModel> Planos { get; set; }
        public int? PagamentoPixId { get; set; }
        public string QRCode { get; set; }
        public string Empresa { get; internal set; }
    }
    public class GerarQrCodePixViewModel
    {
        public int PagamentoPixId { get; set; }
        public string QRCodePix { get; set; }
        public string TipoPlano { get; set; }
    }

}