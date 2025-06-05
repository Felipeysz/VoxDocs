using Microsoft.EntityFrameworkCore;
using VoxDocs.Models;

namespace VoxDocs.Data
{
    public class VoxDocsContext : DbContext
    {
        public VoxDocsContext(DbContextOptions<VoxDocsContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<PastaPrincipalModel> PastaPrincipal { get; set; }
        public DbSet<SubPastaModel> SubPastas { get; set; }
        public DbSet<DocumentoModel> Documentos { get; set; }
        public DbSet<PlanosVoxDocsModel> PlanosVoxDocs { get; set; }
        public DbSet<EmpresasContratanteModel> EmpresasContratantes { get; set; }
        public DbSet<PagamentoCartaoFalsoModel> PagamentosCartao { get; set; }
        public DbSet<PagamentoPixModel> PagamentosPix { get; set; }
        public DbSet<PagamentoConcluido> PagamentosConcluidos { get; set; }

        // Novos DbSets para a funcionalidade de SuporteVoxDocs
        public DbSet<ChamadoModel> Chamados { get; set; }
        public DbSet<MensagemModel> Mensagens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed predefined plans
            modelBuilder.Entity<PlanosVoxDocsModel>().HasData(
            // Plano Gratuito
            new PlanosVoxDocsModel
            {
                Id = 1,
                Name = "Gratuito",
                Description = "Plano com funcionalidades básicas",
                Price = 0m,
                Duration = 0, // 0 = infinito
                Periodicidade = "Ilimitada",
                ArmazenamentoDisponivel = 10,
                TokensDisponiveis = "Limitado",
                LimiteAdmin = 2,
                LimiteUsuario = 5
            },

            // Plano Premium base para 1 mês (sem desconto)
            new PlanosVoxDocsModel
            {
                Id = 2,
                Name = "Premium",
                Description = "Plano completo com funcionalidades avançadas",
                Price = 29.99m,
                Duration = 1,
                Periodicidade = "Mensal",
                ArmazenamentoDisponivel = 200,
                TokensDisponiveis = "Infinito",
                LimiteAdmin = -1, // -1 = Ilimitado
                LimiteUsuario = -1
            },

            // Plano Premium 6 meses com 10% de desconto
            new PlanosVoxDocsModel
            {
                Id = 3,
                Name = "Premium",
                Description = "Plano completo com desconto de 10% para 6 meses",
                Price = Math.Round(29.99m * 6 * 0.9m, 2), // ~161.95
                Duration = 6,
                Periodicidade = "Semestral",
                ArmazenamentoDisponivel = 200,
                TokensDisponiveis = "Infinito",
                LimiteAdmin = -1,
                LimiteUsuario = -1
            },

            // Plano Premium 12 meses com 20% de desconto
            new PlanosVoxDocsModel
            {
                Id = 4,
                Name = "Premium",
                Description = "Plano completo com desconto de 20% para 12 meses",
                Price = Math.Round(29.99m * 12 * 0.8m, 2), // ~287.90
                Duration = 12,
                Periodicidade = "Anual",
                ArmazenamentoDisponivel = 200,
                TokensDisponiveis = "Infinito",
                LimiteAdmin = -1,
                LimiteUsuario = -1
            }
        );

            // Configuração do relacionamento 1:N entre Chamado e Mensagem
            modelBuilder.Entity<MensagemModel>()
                .HasOne(m => m.Chamado)
                .WithMany(c => c.Mensagens)
                .HasForeignKey(m => m.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
