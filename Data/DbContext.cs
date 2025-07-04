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
        public DbSet<PlanosVoxDocsModel> PlanosVoxDocs { get; set; }
        public DbSet<EmpresasContratanteModel> EmpresasContratantes { get; set; }
        public DbSet<PastaPrincipalModel> PastasPrincipais { get; set; }
        public DbSet<SubPastaModel> SubPastas { get; set; }
        public DbSet<DocumentoModel> Documentos { get; set; }
        public DbSet<PagamentoAtivoModel> Pagamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para UserModel
            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.Property(u => u.LimiteUsuario)
                    .HasMaxLength(20)
                    .HasDefaultValue("0")
                    .IsRequired(false);

                entity.Property(u => u.LimiteAdmin)
                    .HasMaxLength(20)
                    .HasDefaultValue("0")
                    .IsRequired(false);
            });

            // Configuração das relações para DocumentoModel
            modelBuilder.Entity<DocumentoModel>(entity =>
            {
                entity.HasOne(d => d.PastaPrincipal)
                    .WithMany()
                    .HasForeignKey(d => d.IdPastaPrincipal)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SubPasta)
                    .WithMany()
                    .HasForeignKey(d => d.IdSubPasta)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Empresa)
                    .WithMany()
                    .HasForeignKey(d => d.IdNomeEmpresa)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuração das relações para PastaPrincipalModel
            modelBuilder.Entity<PastaPrincipalModel>(entity =>
            {
                entity.HasOne(p => p.Empresa)
                    .WithMany(e => e.PastasPrincipais)
                    .HasForeignKey(p => p.EmpresaId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.SubPastas)
                    .WithOne(s => s.PastaPrincipal)
                    .HasForeignKey(s => s.PastaPrincipalId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuração das relações para SubPastaModel
            modelBuilder.Entity<SubPastaModel>(entity =>
            {
                entity.HasOne(s => s.PastaPrincipal)
                    .WithMany(p => p.SubPastas)
                    .HasForeignKey(s => s.PastaPrincipalId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Seed predefined plans
            modelBuilder.Entity<PlanosVoxDocsModel>().HasData(
                new PlanosVoxDocsModel
                {
                    Id = Guid.Parse("f9b2f7e0-d938-4b4d-b256-38bbd6a9d4ef"),
                    Nome = "Gratuito",
                    Descriçao = "Plano com funcionalidades básicas",
                    Preco = 0m,
                    Duracao = 0,
                    Periodicidade = "Ilimitado",
                    ArmazenamentoDisponivel = 200,
                    LimiteAdmin = 2,
                    LimiteUsuario = 5
                },
                new PlanosVoxDocsModel
                {
                    Id = Guid.Parse("b40c1b56-6cc2-4988-b979-3b00c1dd8e1e"),
                    Nome = "Premium",
                    Descriçao = "Plano completo com funcionalidades avançadas",
                    Preco = 89.90m,
                    Duracao = 1,
                    Periodicidade = "Mensal",
                    ArmazenamentoDisponivel = 2500,
                    LimiteAdmin = 999,
                    LimiteUsuario = 999
                },
                new PlanosVoxDocsModel
                {
                    Id = Guid.Parse("7d7f02e7-44b5-4692-88a4-8c2b149b5059"),
                    Nome = "Premium Semestral",
                    Descriçao = "Plano completo com 10% de desconto (6 meses)",
                    Preco = Math.Round(119.90m * 6 * 0.9m, 2),
                    Duracao = 6,
                    Periodicidade = "Semestral",
                    ArmazenamentoDisponivel = 2700,
                    LimiteAdmin = 999,
                    LimiteUsuario = 999
                },
                new PlanosVoxDocsModel
                {
                    Id = Guid.Parse("0e8c6b83-49c1-403e-b70c-6fc8e0f09c7f"),
                    Nome = "Premium Anual",
                    Descriçao = "Plano completo com 10% de desconto (12 meses)",
                    Preco = Math.Round(129.90m * 12 * 0.9m, 2),
                    Duracao = 12,
                    Periodicidade = "Anual",
                    ArmazenamentoDisponivel = 3000,
                    LimiteAdmin = 999,
                    LimiteUsuario = 999
                }
            );

            // Seed da empresa VoxDocs
            var empresaVoxDocsId = Guid.Parse("084498f8-c589-44e9-b804-38b00abca89a");
            modelBuilder.Entity<EmpresasContratanteModel>().HasData(
                new EmpresasContratanteModel
                {
                    Id = empresaVoxDocsId,
                    PlanoId = Guid.Parse("0e8c6b83-49c1-403e-b70c-6fc8e0f09c7f"),
                    EmpresaNome = "VoxDocs",
                    EmailEmpresa = "VoxDocs@gmail.com",
                    DataCriacao = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    PlanoAtivo = true,
                    DataExpiracao = new DateTime(2034, 1, 1), // 10 anos de validade
                }
            );

            // Seed das contas de suporte
            var suporteAdminId = Guid.Parse("46155276-4424-43f7-8290-19d80a29952c");
            var suporteId = Guid.Parse("7be07dc5-5c2d-40d1-a3b5-63d4b838620d");

            modelBuilder.Entity<UserModel>().HasData(
                new UserModel
                {
                    Id = suporteAdminId,
                    Usuario = "SuporteVoxDocsAdmin",
                    Email = "suporte.admin@voxdocs.com",
                    Senha = "Admin@Vox123!", // ATENÇÃO: Substituir por hash em produção
                    PermissionAccount = "SuporteVoxDocsAdmin",
                    EmpresaContratante = "VoxDocs",
                    DataCriacao = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    Ativo = true,
                    PlanoPago = "Premium",
                    LimiteUsuario = "9999",
                    LimiteAdmin = "9999",
                    UltimoLogin = null,
                    PasswordResetToken = null,
                    PasswordResetTokenExpiration = null
                },
                new UserModel
                {
                    Id = suporteId,
                    Usuario = "SuporteVoxDocs",
                    Email = "suporte@voxdocs.com",
                    Senha = "Suporte@Vox456!", // ATENÇÃO: Substituir por hash em produção
                    PermissionAccount = "SuporteVoxDocs",
                    EmpresaContratante = "VoxDocs",
                    DataCriacao = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    Ativo = true,
                    PlanoPago = "Premium",
                    LimiteUsuario = "9999",
                    LimiteAdmin = "9999",
                    UltimoLogin = null,
                    PasswordResetToken = null,
                    PasswordResetTokenExpiration = null
                }
            );

            // Seed de pasta principal para a VoxDocs
            modelBuilder.Entity<PastaPrincipalModel>().HasData(
                new PastaPrincipalModel
                {
                    Id = Guid.Parse("7f8ef49a-8833-4239-8189-2e1947fc8d8b"),
                    NomePastaPrincipal = "Principal",
                    EmpresaId = empresaVoxDocsId,
                    DataCriacao = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        // Método para garantir que os dados iniciais existam
        public async Task EnsureInitialDataExistsAsync()
        {
            // Verificar e criar a empresa VoxDocs se não existir
            var empresaExists = await EmpresasContratantes.AnyAsync(e => e.EmpresaNome == "VoxDocs");
            if (!empresaExists)
            {
                var empresaVoxDocs = new EmpresasContratanteModel
                {
                    Id = Guid.Parse("d6672801-0bd3-431c-8bba-8e1e84ded124"),
                    PlanoId = Guid.Parse("0e8c6b83-49c1-403e-b70c-6fc8e0f09c7f"),
                    EmpresaNome = "VoxDocs",
                    EmailEmpresa = "VoxDocs@gmail.com",
                    DataCriacao = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    PlanoAtivo = true,
                    DataExpiracao = DateTime.UtcNow.AddYears(10), // 10 anos de validade
                };
                await EmpresasContratantes.AddAsync(empresaVoxDocs);
            }

            // Verificar e criar as contas de suporte
            var adminExists = await Users.AnyAsync(u => u.PermissionAccount == "SuporteVoxDocsAdmin");
            var supportExists = await Users.AnyAsync(u => u.PermissionAccount == "SuporteVoxDocs");

            if (!adminExists || !supportExists)
            {
                var suporteAdmin = new UserModel
                {
                    Id = Guid.Parse("5b88c74e-9c49-4f1a-af19-820bb16b1883"),
                    Usuario = "SuporteVoxDocsAdmin",
                    Email = "suporte.admin@voxdocs.com",
                    Senha = "Admin@Vox123!", // ATENÇÃO: Substituir por hash em produção
                    PermissionAccount = "SuporteVoxDocsAdmin",
                    EmpresaContratante = "VoxDocs",
                    DataCriacao = new DateTime(2024, 1, 1),
                    Ativo = true,
                    PlanoPago = "Premium",
                    LimiteUsuario = "0",
                    LimiteAdmin = "0"
                };

                var suporte = new UserModel
                {
                    Id = Guid.Parse("dc2ef266-ba4e-4999-b1b3-f176dacf7d4b"),
                    Usuario = "SuporteVoxDocs",
                    Email = "suporte@voxdocs.com",
                    Senha = "Suporte@Vox456!", // ATENÇÃO: Substituir por hash em produção
                    PermissionAccount = "SuporteVoxDocs",
                    EmpresaContratante = "VoxDocs",
                    DataCriacao = new DateTime(2024, 1, 1),
                    Ativo = true,
                    PlanoPago = "Premium",
                    LimiteUsuario = "0",
                    LimiteAdmin = "0"
                };

                if (!adminExists) await Users.AddAsync(suporteAdmin);
                if (!supportExists) await Users.AddAsync(suporte);
            }

            // Verificar e criar pasta principal para VoxDocs
            var empresaVoxDocsId = Guid.Parse("834ab930-8213-478a-9fd5-e05eee7374c3");
            var pastaExists = await PastasPrincipais.AnyAsync(p => p.EmpresaId == empresaVoxDocsId);
            if (!pastaExists)
            {
                var pastaPrincipal = new PastaPrincipalModel
                {
                    Id = Guid.Parse("554fdc51-b6d7-418b-a27b-62392a267e2a"),
                    NomePastaPrincipal = "Principal",
                    EmpresaId = empresaVoxDocsId,
                    DataCriacao = DateTime.UtcNow
                };
                await PastasPrincipais.AddAsync(pastaPrincipal);
            }

            await SaveChangesAsync();
        }
    }
}

