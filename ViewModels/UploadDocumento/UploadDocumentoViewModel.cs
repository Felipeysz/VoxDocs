using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VoxDocs.DTO;

namespace VoxDocs.Models.ViewModels
{
    public class UploadDocumentoViewModel
    {
        [Required(ErrorMessage = "Por favor, selecione um arquivo")]
        [Display(Name = "Arquivo")]
        public IFormFile Arquivo { get; set; }

        [Required(ErrorMessage = "Por favor, selecione uma pasta principal")]
        [Display(Name = "Pasta Principal")]
        public Guid? SelectedPastaPrincipalId { get; set; }

        [Display(Name = "Subpasta")]
        public Guid? SelectedSubPastaId { get; set; }

        [Required(ErrorMessage = "Por favor, selecione o nível de segurança")]
        [Display(Name = "Nível de Segurança")]
        public string NivelSeguranca { get; set; } = "Publico";

        [Display(Name = "Token de Segurança")]
        public string? TokenSeguranca { get; set; }

        [Required(ErrorMessage = "Por favor, insira uma descrição")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        // Listas para dropdowns (não devem ser validadas no POST)
        [BindNever] // Adicione esta anotação
        public List<PastaPrincipalDTO> PastaPrincipais { get; set; }

        [BindNever] // Adicione esta anotação
        public IEnumerable<SubPastaCreateDTO>? SubPastas { get; set; }

        // Lista de níveis de segurança disponíveis
        [BindNever] // Adicione esta anotação
        public List<SelectListItem> NiveisSeguranca => new List<SelectListItem>
        {
            new SelectListItem { Value = "Publico", Text = "Público (Nível 1)" },
            new SelectListItem { Value = "Restrito", Text = "Restrito (Nível 2)" },
            new SelectListItem { Value = "Confidencial", Text = "Confidencial (Nível 3)" },
            new SelectListItem { Value = "Prioritario", Text = "Prioritário (Nível 4)" },
            new SelectListItem { Value = "Urgente", Text = "Urgente (Nível 5)" }
        };

        // Flag para admin
        [BindNever] // Adicione esta anotação
        public bool IsAdmin { get; set; }
    }
}