// Models/ViewModels/UploadDocumentoViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using VoxDocs.DTO;

namespace VoxDocs.Models.ViewModels
{
    public class UploadDocumentoViewModel
    {
        [Required(ErrorMessage = "O arquivo é obrigatório")]
        public IFormFile Arquivo { get; set; }

        [Required(ErrorMessage = "Selecione uma categoria principal")]
        public int SelectedPastaPrincipalId { get; set; }

        [Required(ErrorMessage = "Selecione uma subcategoria")]
        public int SelectedSubPastaId { get; set; }

        [Required(ErrorMessage = "Escolha um nível de segurança")]
        public string NivelSeguranca { get; set; }

        // Não marcar como Required aqui
        public string? TokenSeguranca { get; set; }

        [Required(ErrorMessage = "Descrição é obrigatória")]
        public string Descricao { get; set; }

        // These should not be marked as required
        public IEnumerable<DTOPastaPrincipal>? PastaPrincipais { get; set; }
        public IEnumerable<DTOSubPasta>? SubPastas { get; set; }
    }
}