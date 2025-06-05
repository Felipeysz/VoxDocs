using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public class EmpresasContratanteService : IEmpresasContratanteService
    {
        private readonly VoxDocsContext _context;
        private readonly IUserService _userService;

        public EmpresasContratanteService(VoxDocsContext context, IUserService userService)
        {
            _userService = userService;
            _context = context;
        }

        public async Task<List<EmpresasContratanteModel>> GetAllAsync()
        {
            return await _context.EmpresasContratantes.ToListAsync();
        }

        public async Task<EmpresasContratanteModel> GetByIdAsync(int id)
        {
            return await _context.EmpresasContratantes.FindAsync(id)
                ?? throw new KeyNotFoundException("Empresa não encontrada.");
        }

        public async Task<EmpresasContratanteModel> CreateAsync(DTOEmpresasContratante dto)
        {
            // Verificar se empresa já existe
            var existing = await _context.EmpresasContratantes
                .FirstOrDefaultAsync(e => e.EmpresaContratante == dto.EmpresaContratante);
            
            if (existing != null)
            {
                throw new InvalidOperationException("Empresa já cadastrada");
            }

            var empresa = new EmpresasContratanteModel
            {
                EmpresaContratante = dto.EmpresaContratante,
                Email = dto.Email
            };
            
            _context.EmpresasContratantes.Add(empresa);
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task<EmpresasContratanteModel> UpdateAsync(int id, DTOEmpresasContratante dto)
        {
            var empresa = await _context.EmpresasContratantes.FindAsync(id)
                ?? throw new KeyNotFoundException("Empresa não encontrada.");

            empresa.EmpresaContratante = dto.EmpresaContratante;
            empresa.Email = dto.Email;

            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task DeleteAsync(int id)
        {
            var empresa = await _context.EmpresasContratantes.FindAsync(id)
                ?? throw new KeyNotFoundException("Empresa não encontrada.");

            _context.EmpresasContratantes.Remove(empresa);
            await _context.SaveChangesAsync();
        }
        
        public async Task<DTOEmpresasContratantePlano> GetPlanoByEmpresaAsync(string nomeEmpresa)
        {
            // Pega todos os usuários daquela empresa
            var users = await _userService.GetUsersAsync();
            var any = users.FirstOrDefault(u => u.EmpresaContratante == nomeEmpresa);

            if (any == null)
                throw new KeyNotFoundException($"Nenhum usuário encontrado para a empresa '{nomeEmpresa}'.");

            // Retorna o DTO com o plano encontrado
            return new DTOEmpresasContratantePlano
            {
                EmpresaContratante = nomeEmpresa,
                TipoPlano = any.PlanoPago ?? "Não atribuído"
            };
        }

    }
}