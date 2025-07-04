using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxDocs.Models
{
    public class DocumentoModel
    {
        public Guid Id { get; set; }
        public string NomeArquivo { get; set; }
        public string DescricaoArquivo { get; set; }
        public long TamanhoDocumento { get; set; }
        public int NivelPrioridade { get; set; }
        public string? TokenAcesso { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public string UsuarioCriacao { get; set; }

        // Relacionamentos
        public Guid IdNomeEmpresa { get; set; }
        public EmpresasContratanteModel Empresa { get; set; }

        public Guid IdPastaPrincipal { get; set; }
        public PastaPrincipalModel PastaPrincipal { get; set; }

        public Guid? IdSubPasta { get; set; }
        public SubPastaModel? SubPasta { get; set; }

        public string BlobName { get; set; }
        public string? ContentType { get; set; }

    }
}