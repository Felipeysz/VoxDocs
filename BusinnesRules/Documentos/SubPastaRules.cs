using System;
using System.Linq;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.BusinessRules
{
    public class SubPastaBusinessRules
    {
        private readonly IPastaPrincipalService _pastaService;
        private readonly ISubPastaService _subService;
        private readonly IEmpresasContratanteService _empresasService;

        public SubPastaBusinessRules(
            IPastaPrincipalService pastaService,
            ISubPastaService subService,
            IEmpresasContratanteService empresasService)
        {
            _pastaService = pastaService;
            _subService = subService;
            _empresasService = empresasService;
        }

        /// <summary>
        /// Valida a criação de uma subpasta.
        /// </summary>
        public async Task ValidateCreateAsync(DTOSubPastaCreate dto)
        {
            if (dto == null)
                throw new ArgumentException("Dados da subpasta inválidos.");

            if (string.IsNullOrWhiteSpace(dto.NomeSubPasta))
                throw new ArgumentException("Nome da subpasta é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.EmpresaContratante))
                throw new ArgumentException("Empresa é obrigatória.");

            // Verifica se a empresa existe
            var empresaExiste = await _empresasService.GetAllAsync();
            if (!empresaExiste.Any(e => e.EmpresaContratante.Equals(dto.EmpresaContratante, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Empresa informada não existe.");
            }

            // Verifica se já existe uma subpasta com o mesmo nome e empresa na pasta principal
            var subPastasExistentes = await _subService.GetAllAsync();
            if (subPastasExistentes.Any(sp =>
                sp.NomeSubPasta.Equals(dto.NomeSubPasta, StringComparison.OrdinalIgnoreCase) &&
                sp.EmpresaContratante.Equals(dto.EmpresaContratante, StringComparison.OrdinalIgnoreCase) &&
                sp.NomePastaPrincipal.Equals(dto.NomePastaPrincipal, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Já existe uma subpasta “{dto.NomeSubPasta}” nesta PastaPrincipal.");
            }
        }
    }
}