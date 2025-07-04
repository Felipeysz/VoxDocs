using System.Collections.Generic;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Models.ViewModels
{
    public class DocumentosViewModel
    {
        // Propriedades para navegação
        public IEnumerable<PastaPrincipalDTO> PastasPrincipais { get; set; }
        public IEnumerable<SubPastaModel> SubPastas { get; set; }
        public IEnumerable<DocumentoModel> Documentos { get; set; }

        // IDs para controle de estado
        public Guid? PastaPrincipalSelecionada { get; set; }
        public Guid? SubPastaSelecionada { get; set; }

        // Informações do usuário
        public bool IsAdmin { get; set; }

        // Propriedades calculadas para a view
        public bool MostrarSubPastas => PastaPrincipalSelecionada.HasValue && !SubPastaSelecionada.HasValue;
        public bool MostrarDocumentos => PastaPrincipalSelecionada.HasValue;
        public string NomePastaPrincipalAtual =>
            PastasPrincipais.FirstOrDefault(p => p.Id == PastaPrincipalSelecionada)?.NomePastaPrincipal;
        public string NomeSubPastaAtual =>
            SubPastas.FirstOrDefault(s => s.Id == SubPastaSelecionada)?.NomeSubPasta;
    }
}