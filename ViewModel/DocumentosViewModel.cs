// Models/ViewModels/DocumentosViewModel.cs
using System.Collections.Generic;
using VoxDocs.DTO;
using VoxDocs.Models.Dto;

namespace VoxDocs.Models.ViewModels
{
    public class DocumentosViewModel
    {
        public IEnumerable<DTOPastaPrincipal> PastaPrincipais { get; set; } = new List<DTOPastaPrincipal>();

        public IEnumerable<DTOSubPasta> SubPastas { get; set; } = new List<DTOSubPasta>();

        public IEnumerable<DTODocumentoCreate> Documentos { get; set; } = new List<DTODocumentoCreate>();
        public string? SelectedPastaPrincipalNome { get; set; }
        public string? SelectedSubPastaNome { get; set; }
    }
    
    public class EditDocumentoViewModel
    {
        public int Id { get; set; }
        public IFormFile Arquivo { get; set; }
        public string Descricao { get; set; }
        public string Token { get; set; } // Token para documentos não públicos
    }
}
