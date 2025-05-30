using Microsoft.AspNetCore.Http;
using System;

namespace VoxDocs.DTO
{
    public class DTODocumentoCreate
    {
        public int Id { get; set; }
        public required string NomeArquivo { get; set; }
        public required string UrlArquivo { get; set; }
        public required string UsuarioCriador { get; set; }
        public DateTime DataCriacao { get; set; }
        public required string UsuarioUltimaAlteracao { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
        public required string EmpresaContratante { get; set; }
        public required string NomePastaPrincipal { get; set; }
        public required string NomeSubPasta { get; set; }
        public long TamanhoArquivo { get; set; }
        public required string NivelSeguranca { get; set; }
        public string? TokenSeguranca { get; set; } // Oculto na resposta
        public required string Descrição { get; set; }
    }
    
    public class DocumentoDto
    {
        public int Id { get; set; }
        public string? NomeArquivo { get; set; }
        public required IFormFile Arquivo { get; set; }
        public required string Usuario { get; set; }
        public required string EmpresaContratante { get; set; }
        public required string NomePastaPrincipal { get; set; }
        public required string NomeSubPasta { get; set; }
        public required string NivelSeguranca { get; set; }
        public string? TokenSeguranca { get; set; }
        public required string Descrição { get; set; }
        public string? UsuarioUltimaAlteracao { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
    }
    
    // NOVO DTO PARA ATUALIZAÇÃO
    public class DocumentoUpdateDto
    {
        public int Id { get; set; }
        public required IFormFile NovoArquivo { get; set; }
        public required string UsuarioUltimaAlteracao { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
        public required string Descrição { get; set; }
    }
    
    public class DTOQuantidadeDocumentoEmpresa
    {
        public required string EmpresaContratante { get; set; }
        public int Quantidade { get; set; }
        public double TamanhoTotalGb { get; set; }
    }
    
    public class DTOAcessosDocumento
    {
        public required string NomeArquivo { get; set; }
        public required string NomeSubPasta { get; set; }
        public required string NomePastaPrincipal { get; set; }
        public int QuantidadeAcessos { get; set; }
    }
    
    public class ValidationResult
    {
        public bool sucesso { get; set; }
    }
    
    public class ErrorResult
    {
        public required string mensagem { get; set; }
    }
}