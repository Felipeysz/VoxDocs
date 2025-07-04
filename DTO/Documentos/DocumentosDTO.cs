using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace VoxDocs.DTO
{
    // Interface comum para garantir que ambos DTOs tenham UsuarioCriacao
    public interface IUsuarioCriacao
    {
        string UsuarioCriacao { get; set; }
    }

    public class DocumentoCreateDTO : IUsuarioCriacao
    {
        public Guid Id { get; set; }
        public string NomeArquivo { get; set; }
        public string DescricaoArquivo { get; set; }
        public Guid IdPastaPrincipal { get; set; }
        public Guid? IdSubPasta { get; set; }
        public int NivelPrioridade { get; set; }
        public string? TokenAcesso { get; set; }
        public string UsuarioCriacao { get; set; }
        public string ContentType { get; internal set; }
        public DateTime DataCriacao { get; set; }
    }

    public class DocumentoUpdateDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome do arquivo é obrigatório")]
        public string NomeArquivo { get; set; }

        public string DescricaoArquivo { get; set; }

        [Required(ErrorMessage = "O nível de prioridade é obrigatório")]
        [Range(1, 5, ErrorMessage = "Nível de prioridade inválido")]
        public int NivelPrioridade { get; set; }

        [StringLength(50, MinimumLength = 10, ErrorMessage = "O token deve ter entre 10 e 50 caracteres")]
        public string TokenAtual { get; set; }

        [StringLength(50, MinimumLength = 10, ErrorMessage = "O novo token deve ter entre 10 e 50 caracteres")]
        public string NovoToken { get; set; }

        public string ConfirmarToken { get; set; }

        public IFormFile Arquivo { get; set; }

        public string UsuarioCriacao { get; set; }
    }

    public class DocumentoDownloadDTO
    {
        public string NomeArquivo { get; set; }
        public byte[] Conteudo { get; set; }
        public string TipoConteudo { get; set; }
    }

    public class DocumentoDetalhesDTO
    {
        public Guid Id { get; set; }
        public string NomeArquivo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public string UsuarioCriacao { get; set; }
        public int NivelPrioridade { get; set; }
        public long TamanhoBytes { get; set; }
        public string TipoArquivo { get; set; }
    }
}