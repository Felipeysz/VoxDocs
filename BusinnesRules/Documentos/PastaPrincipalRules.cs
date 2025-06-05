using System;
using System.Linq;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.BusinessRules
{
    public class PastaPrincipalBusinessRules
    {
        private readonly IEmpresasContratanteService _empresasService;
        private readonly IPastaPrincipalService _pastaService;

        public PastaPrincipalBusinessRules(
            IEmpresasContratanteService empresasService,
            IPastaPrincipalService pastaService)
        {
            _empresasService = empresasService;
            _pastaService = pastaService;
        }

        /// <summary>
        /// Valida a criação de uma pasta principal.
        /// </summary>
        public async Task ValidateCreateAsync(DTOPastaPrincipalCreate dto)
        {
            if (dto == null)
                throw new ArgumentException("Dados da pasta inválidos.");

            if (string.IsNullOrWhiteSpace(dto.NomePastaPrincipal))
                throw new ArgumentException("Nome da pasta é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.EmpresaContratante))
                throw new ArgumentException("Empresa é obrigatória.");

            // Verifica se a empresa existe
            var empresaExiste = await _empresasService.GetAllAsync();
            if (!empresaExiste.Any(e => e.EmpresaContratante.Equals(dto.EmpresaContratante, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Empresa informada não existe.");
            }

            // Verifica se já existe uma pasta principal com o mesmo nome e empresa
            var pastasExistentes = await _pastaService.GetAllAsync();
            if (pastasExistentes.Any(p =>
                p.NomePastaPrincipal.Equals(dto.NomePastaPrincipal, StringComparison.OrdinalIgnoreCase) &&
                p.EmpresaContratante.Equals(dto.EmpresaContratante, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Já existe uma pasta principal “{dto.NomePastaPrincipal}” para a empresa “{dto.EmpresaContratante}”.");
            }
        }
    }
}