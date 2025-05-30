using VoxDocs.Models;
using VoxDocs.DTO;
using VoxDocs.Services;
using System.Threading.Tasks;
using System;

namespace VoxDocs.BusinessRules
{
    public class DocumentoBusinessRules
    {
        private readonly IDocumentoService _documentoService;
        private readonly List<string> _allowedExtensions = new List<string> 
        { 
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", 
            ".txt", ".jpg", ".jpeg", ".png", ".gif" 
        };

        public DocumentoBusinessRules(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        public async Task ValidateDocumentoUploadAsync(DocumentoDto dto)
        {
            if (dto.Arquivo == null)
                throw new ArgumentException("Arquivo é obrigatório");

            if (string.IsNullOrWhiteSpace(dto.EmpresaContratante))
                throw new ArgumentException("Empresa é obrigatória");

            if (string.IsNullOrWhiteSpace(dto.NomePastaPrincipal))
                throw new ArgumentException("Nome da pasta principal é obrigatório");

            if (string.IsNullOrWhiteSpace(dto.NivelSeguranca))
                throw new ArgumentException("Nível de segurança é obrigatório");

            if (!IsValidSecurityLevel(dto.NivelSeguranca))
                throw new ArgumentException("Nível de segurança inválido");

            if (dto.Arquivo.Length > 100 * 1024 * 1024) // 100MB limit
                throw new ArgumentException("Tamanho máximo do arquivo excedido (100MB)");

            if (dto.NivelSeguranca != "Publico" && string.IsNullOrEmpty(dto.TokenSeguranca))
                throw new ArgumentException("Token de segurança é obrigatório para documentos restritos ou confidenciais");

                // Validação de duplicidade no Blob
            var fileName = Path.GetFileName(dto.Arquivo.FileName);
            if (await _documentoService.ArquivoExisteAsync(fileName))
            throw new ArgumentException($"Documento '{fileName}' já existe no sistema.");

            // Validação de duplicidade no banco de dados
            var documentosExistentes = await _documentoService.GetAllAsync();
            if (documentosExistentes.Any(d => d.NomeArquivo.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Documento '{fileName}' já existe no sistema.");
        }

        public async Task ValidateDocumentoUpdateAsync(DocumentoUpdateDto dto, string tokenSeguranca = null)
        {
            if (dto.Id <= 0)
                throw new ArgumentException("ID do documento inválido");

            // Valida existência do documento
            var documento = await _documentoService.GetByIdAsync(dto.Id);
            if (documento == null)
                throw new ArgumentException("Documento não encontrado");

            // Valida novo arquivo se fornecido
            if (dto.NovoArquivo != null && dto.NovoArquivo.Length > 0)
            {
                // Valida tamanho do arquivo (100MB)
                if (dto.NovoArquivo.Length > 100 * 1024 * 1024)
                    throw new ArgumentException("Tamanho máximo do arquivo excedido (100MB)");
                
                // Valida tipo de arquivo (opcional)
                var extension = Path.GetExtension(dto.NovoArquivo.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    throw new ArgumentException($"Tipo de arquivo não permitido: {extension}");
            }

            // Valida descrição
            if (string.IsNullOrWhiteSpace(dto.Descrição))
                throw new ArgumentException("Descrição é obrigatória");

            // Valida usuário
            if (string.IsNullOrWhiteSpace(dto.UsuarioUltimaAlteracao))
                throw new ArgumentException("Usuário da última alteração é obrigatório");

            // Valida nível de segurança para documentos confidenciais
            if (documento.NivelSeguranca == "Confidencial" && 
                string.IsNullOrWhiteSpace(tokenSeguranca))
            {
                throw new ArgumentException("Token de segurança é obrigatório para documentos confidenciais");
            }
        }

        public async Task ValidateDocumentoUpdateAsync(DocumentoModel documento)
        {
            if (documento == null)
                throw new ArgumentException("Documento inválido");

            if (string.IsNullOrWhiteSpace(documento.NomeArquivo))
                throw new ArgumentException("Nome do arquivo é obrigatório");

            if (string.IsNullOrWhiteSpace(documento.UrlArquivo))
                throw new ArgumentException("URL do arquivo é obrigatória");

            if (!IsValidSecurityLevel(documento.NivelSeguranca))
                throw new ArgumentException("Nível de segurança inválido");
        }

        private bool IsValidSecurityLevel(string nivelSeguranca)
        {
            var validLevels = new[] { "Publico", "Restrito", "Confidencial" };
            return validLevels.Contains(nivelSeguranca);
        }

        public async Task CheckDocumentoExistsAsync(int Id)
        {
            var doc = await _documentoService.GetByIdAsync(Id);
            if (doc == null)
            {
                throw new ArgumentException("Documento não encontrado.");
            }
        }
       public async Task ValidateDocumentoDeleteAsync(int id, string token = null)
        {
            var doc = await _documentoService.GetByIdAsync(id);
            if (doc == null)
            {
                throw new ArgumentException("Documento não encontrado.");
            }

            // Verifica se o documento não é público e se o token foi fornecido
            if (doc.NivelSeguranca != "Publico" && string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token de segurança é obrigatório para documentos restritos ou confidenciais.");
            }

            // Verifica se o token fornecido é válido
            if (doc.NivelSeguranca != "Publico" && doc.TokenSeguranca != token)
            {
                throw new UnauthorizedAccessException("Token de segurança inválido.");
            }
        }
        public async Task ValidateDocumentoAccessAsync(int Id, string usuario, string? token)
        {
            var documento = await _documentoService.GetByIdAsync(Id);
            if (documento == null)
                throw new ArgumentException("Documento não encontrado");

            if (documento.NivelSeguranca == "Confidencial" && !usuario.EndsWith("@admin.com"))
                throw new UnauthorizedAccessException("Acesso negado a documentos com nível de segurança 'Confidencial'");

            if (documento.NivelSeguranca != "Publico" && documento.TokenSeguranca != token)
                throw new UnauthorizedAccessException("Token de segurança inválido");
        }
    }
}