using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;
using VoxDocs.DTO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System;
using System.IO;

namespace VoxDocs.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly VoxDocsContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public DocumentoService(VoxDocsContext context, IConfiguration configuration)
        {
            _context = context;
            var ConnectionString = configuration["AzureBlobStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(ConnectionString);
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        private string GenerateTokenHash(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(token);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<bool> ValidateTokenDocumentoAsync(string nomeArquivo, string token)
        {
            var doc = await _context.Documentos
                .FirstOrDefaultAsync(d => d.NomeArquivo.ToLower() == nomeArquivo.ToLower());

            if (doc == null)
                throw new ArgumentException("Documento não encontrado.");

            if (doc.NivelSeguranca == "Publico")
                return true;

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Token de segurança é obrigatório para este documento.");

            token = token.Trim();
            string hashedToken = GenerateTokenHash(token);
            
            if (doc.TokenSeguranca == hashedToken)
                return true;

            throw new UnauthorizedAccessException("Token de segurança inválido.");
        }

        public async Task<DTODocumentoCreate> GetByIdAsync(int id, string token = null)
        {
            var doc = await _context.Documentos.FindAsync(id);
            if (doc == null) throw new ArgumentException("Documento não encontrado.");

            if (doc.NivelSeguranca == "Publico") 
                return MapToDTO(doc);

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Token de segurança é obrigatório para documentos restritos ou confidenciais.");

            token = token.Trim();
            string hashedToken = GenerateTokenHash(token);
            
            if (doc.TokenSeguranca != hashedToken)
                throw new UnauthorizedAccessException("Token de segurança inválido.");

            return MapToDTO(doc);
        }

        public async Task<IEnumerable<DTODocumentoCreate>> GetAllAsync()
        {
            var documentos = await _context.Documentos.ToListAsync();
            return documentos.Select(MapToDTO);
        }

        public async Task<IEnumerable<DTODocumentoCreate>> GetBySubPastaAsync(string subPasta)
        {
            var documentos = await _context.Documentos
                .Where(d => d.NomeSubPasta == subPasta)
                .ToListAsync();
            return documentos.Select(MapToDTO);
        }

        public async Task<IEnumerable<DTODocumentoCreate>> GetByPastaPrincipalAsync(string pastaPrincipal)
        {
            var documentos = await _context.Documentos
                .Where(d => d.NomePastaPrincipal == pastaPrincipal)
                .ToListAsync();
            return documentos.Select(MapToDTO);
        }

        public async Task<DTODocumentoCreate> CreateAsync(DocumentoDto dto)
        {
            try
            {
                // Upload do arquivo para o Blob Storage
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobName = dto.Arquivo.FileName;
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var stream = dto.Arquivo.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: false);
                }

                var url = blobClient.Uri.ToString();

                // Gera hash do token antes de armazenar
                string tokenHash = GenerateTokenHash(dto.TokenSeguranca);

                // Criação do documento no banco de dados
                var doc = new DocumentoModel
                {
                    NomeArquivo = dto.Arquivo.FileName,
                    UrlArquivo = url,
                    UsuarioCriador = dto.Usuario,
                    DataCriacao = ConvertToBrasiliaTime(DateTime.UtcNow),
                    UsuarioUltimaAlteracao = dto.Usuario,
                    DataUltimaAlteracao = ConvertToBrasiliaTime(DateTime.UtcNow),
                    Empresa = dto.EmpresaContratante,
                    NomePastaPrincipal = dto.NomePastaPrincipal,
                    NomeSubPasta = dto.NomeSubPasta,
                    TamanhoArquivo = dto.Arquivo.Length,
                    NivelSeguranca = dto.NivelSeguranca,
                    TokenSeguranca = tokenHash, // Armazena o HASH
                    Descrição = dto.Descrição
                };

                _context.Documentos.Add(doc);
                await _context.SaveChangesAsync();

                return MapToDTO(doc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar documento: {ex.Message}");
                throw;
            }
        }

        private DateTime ConvertToBrasiliaTime(DateTime utcTime)
        {
            try
            {
                var brasiliaZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, brasiliaZone);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao converter para Brasília: {ex.Message}");
                return utcTime;
            }
        }      
        
        public async Task DeleteAsync(int id, string token = null)
        {
            var doc = await _context.Documentos.FindAsync(id);
            if (doc == null)
                throw new ArgumentException("Documento não encontrado.");

            // Validação de token para documentos não públicos
            if (doc.NivelSeguranca != "Publico")
            {
                if (string.IsNullOrWhiteSpace(token))
                    throw new UnauthorizedAccessException("Token obrigatório para documentos restritos.");

                string hashedToken = GenerateTokenHash(token.Trim());
                if (doc.TokenSeguranca != hashedToken)
                    throw new UnauthorizedAccessException("Token inválido.");
            }

            // USO DIRETO DO NOME DO ARQUIVO - ABORDAGEM CONSISTENTE
            var blobName = doc.NomeArquivo;

            // Exclusão do blob
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();

            // Exclusão do registro
            _context.Documentos.Remove(doc);
            await _context.SaveChangesAsync();
        }

        public async Task<DTODocumentoCreate> UpdateAsync(DocumentoUpdateDto dto)
        {
            var doc = await _context.Documentos.FindAsync(dto.Id);
            if (doc == null)
            {
                throw new ArgumentException("Documento não encontrado.");
            }

            // Campos que serão atualizados
            doc.UsuarioUltimaAlteracao = dto.UsuarioUltimaAlteracao;
            doc.DataUltimaAlteracao = ConvertToBrasiliaTime(DateTime.UtcNow);
            doc.Descrição = dto.Descrição;

            // Processa novo arquivo se fornecido
            if (dto.NovoArquivo != null && dto.NovoArquivo.Length > 0)
            {
                // Exclui o blob antigo
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var oldBlobClient = containerClient.GetBlobClient(doc.NomeArquivo);
                await oldBlobClient.DeleteIfExistsAsync();

                // Faz upload do novo blob
                var newBlobClient = containerClient.GetBlobClient(doc.NomeArquivo);
                using (var stream = dto.NovoArquivo.OpenReadStream())
                {
                    await newBlobClient.UploadAsync(stream, overwrite: true);
                }

                // Atualiza o tamanho do arquivo
                doc.TamanhoArquivo = dto.NovoArquivo.Length;
            }

            _context.Documentos.Update(doc);
            await _context.SaveChangesAsync();

            return MapToDTO(doc);
        }

        public async Task<DTOQuantidadeDocumentoEmpresa> GetEstatisticasEmpresaAsync(string empresa)
        {
            var documentos = await _context.Documentos
                .Where(d => d.Empresa == empresa)
                .ToListAsync();

            return new DTOQuantidadeDocumentoEmpresa
            {
                EmpresaContratante = empresa,
                Quantidade = documentos.Count,
                TamanhoTotalGb = documentos.Sum(d => d.TamanhoArquivo) / 1024.0 / 1024.0 / 1024.0
            };
        }

        public async Task<DTOAcessosDocumento> GetAcessosDocumentoAsync(int id, int dias)
        {
            var doc = await _context.Documentos.FindAsync(id);
            if (doc == null) return null;

            return new DTOAcessosDocumento
            {
                NomeArquivo = doc.NomeArquivo,
                NomeSubPasta = doc.NomeSubPasta,
                NomePastaPrincipal = doc.NomePastaPrincipal,
                QuantidadeAcessos = doc.ContadorAcessos
            };
        }

        public async Task IncrementarAcessoAsync(int id)
        {
            var doc = await _context.Documentos.FindAsync(id);
            if (doc != null)
            {
                doc.ContadorAcessos++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ArquivoExisteAsync(string nomeArquivo)
        {
            return await _context.Documentos.AnyAsync(d => d.NomeArquivo == nomeArquivo);
        }

        public async Task<(Stream stream, string contentType)> DownloadDocumentoProtegidoAsync(string nomeArquivo, string token = null)
        {
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.NomeArquivo == nomeArquivo);
            if (doc == null)
                throw new ArgumentException("Documento não encontrado.");

            // Validação com hash
            if (doc.NivelSeguranca != "Publico")
            {
                if (string.IsNullOrWhiteSpace(token))
                    throw new UnauthorizedAccessException("Token de segurança é obrigatório para este documento.");

                token = token.Trim();
                string hashedToken = GenerateTokenHash(token);
                
                if (doc.TokenSeguranca != hashedToken)
                    throw new UnauthorizedAccessException("Token de segurança inválido.");
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(nomeArquivo);

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException("Arquivo não encontrado no Blob Storage.");

            var downloadInfo = await blobClient.DownloadAsync();
            var contentType = downloadInfo.Value.ContentType ?? "application/octet-stream";
            return (downloadInfo.Value.Content, contentType);
        }

        private DTODocumentoCreate MapToDTO(DocumentoModel doc)
        {
            return new DTODocumentoCreate
            {
                Id = doc.Id,
                NomeArquivo = doc.NomeArquivo,
                UrlArquivo = doc.UrlArquivo,
                UsuarioCriador = doc.UsuarioCriador,
                DataCriacao = doc.DataCriacao,
                UsuarioUltimaAlteracao = doc.UsuarioUltimaAlteracao,
                DataUltimaAlteracao = doc.DataUltimaAlteracao,
                EmpresaContratante = doc.Empresa,
                NomePastaPrincipal = doc.NomePastaPrincipal,
                NomeSubPasta = doc.NomeSubPasta,
                TamanhoArquivo = doc.TamanhoArquivo,
                NivelSeguranca = doc.NivelSeguranca,
                TokenSeguranca = doc.TokenSeguranca != null 
                    ? $"{doc.TokenSeguranca.Substring(0, 4)}..." 
                    : null,
                Descrição = doc.Descrição
            };
        }
    }
}