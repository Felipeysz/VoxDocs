using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.ViewModels
{
    public class PlanosViewModel
    {
        public List<PlanosVoxDocsModel> Planos { get; set; } = new List<PlanosVoxDocsModel>();
        public string CategoriaFiltro { get; set; }
        public string MensagemErro { get; set; }
        public int TotalPlanos => Planos.Count;
        public bool IsUserAuthenticated { get; set; }
        public string Username { get; set; }


        // Para formulários de criação/edição
        public DTOPlanosVoxDocs FormularioPlano { get; set; }
        public Guid PlanoIdEdicao { get; set; }
    }

    public class DetalhesPlanoViewModel
    {
        public PlanosVoxDocsModel Plano { get; set; }
        public bool MostrarOpcoesAdmin { get; set; }
        public string MensagemSucesso { get; set; }
        public string MensagemErro { get; set; }
    }
}