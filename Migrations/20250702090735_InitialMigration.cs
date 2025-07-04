using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VoxDocs.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomePlanoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetodoPagamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PastaPrincipalIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuariosCriados = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanosVoxDocs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Periodicidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Duracao = table.Column<int>(type: "int", nullable: true),
                    ArmazenamentoDisponivel = table.Column<int>(type: "int", nullable: true),
                    LimiteAdmin = table.Column<int>(type: "int", nullable: true),
                    LimiteUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosVoxDocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionAccount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpresaContratante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LimiteUsuario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "0"),
                    LimiteAdmin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "0"),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpresasContratante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailEmpresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanoAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasContratante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresasContratante_PlanosVoxDocs_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "PlanosVoxDocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PastasPrincipais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomePastaPrincipal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastasPrincipais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastasPrincipais_EmpresasContratante_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "EmpresasContratante",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubPastas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeSubPasta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PastaPrincipalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPastas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubPastas_PastasPrincipais_PastaPrincipalId",
                        column: x => x.PastaPrincipalId,
                        principalTable: "PastasPrincipais",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeArquivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescricaoArquivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanhoDocumento = table.Column<long>(type: "bigint", nullable: false),
                    NivelPrioridade = table.Column<int>(type: "int", nullable: false),
                    TokenAcesso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNomeEmpresa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPastaPrincipal = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSubPasta = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BlobName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_EmpresasContratante_IdNomeEmpresa",
                        column: x => x.IdNomeEmpresa,
                        principalTable: "EmpresasContratante",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documentos_PastasPrincipais_IdPastaPrincipal",
                        column: x => x.IdPastaPrincipal,
                        principalTable: "PastasPrincipais",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documentos_SubPastas_IdSubPasta",
                        column: x => x.IdSubPasta,
                        principalTable: "SubPastas",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PlanosVoxDocs",
                columns: new[] { "Id", "ArmazenamentoDisponivel", "Descricao", "Duracao", "LimiteAdmin", "LimiteUsuario", "Nome", "Periodicidade", "Preco" },
                values: new object[,]
                {
                    { new Guid("0e8c6b83-49c1-403e-b70c-6fc8e0f09c7f"), 3000, "Plano completo com 10% de desconto (12 meses)", 12, 999, 999, "Premium Anual", "Anual", 1402.92m },
                    { new Guid("7d7f02e7-44b5-4692-88a4-8c2b149b5059"), 2700, "Plano completo com 10% de desconto (6 meses)", 6, 999, 999, "Premium Semestral", "Semestral", 647.46m },
                    { new Guid("b40c1b56-6cc2-4988-b979-3b00c1dd8e1e"), 2500, "Plano completo com funcionalidades avançadas", 1, 999, 999, "Premium", "Mensal", 89.90m },
                    { new Guid("f9b2f7e0-d938-4b4d-b256-38bbd6a9d4ef"), 200, "Plano com funcionalidades básicas", 0, 2, 5, "Gratuito", "Ilimitado", 0m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Email", "EmpresaContratante", "LimiteAdmin", "LimiteUsuario", "PasswordResetToken", "PasswordResetTokenExpiration", "PermissionAccount", "PlanoPago", "Senha", "UltimoLogin", "Usuario" },
                values: new object[,]
                {
                    { new Guid("46155276-4424-43f7-8290-19d80a29952c"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "suporte.admin@voxdocs.com", "VoxDocs", "9999", "9999", null, null, "SuporteVoxDocsAdmin", "Premium", "Admin@Vox123!", null, "SuporteVoxDocsAdmin" },
                    { new Guid("7be07dc5-5c2d-40d1-a3b5-63d4b838620d"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "suporte@voxdocs.com", "VoxDocs", "9999", "9999", null, null, "SuporteVoxDocs", "Premium", "Suporte@Vox456!", null, "SuporteVoxDocs" }
                });

            migrationBuilder.InsertData(
                table: "EmpresasContratante",
                columns: new[] { "Id", "DataCriacao", "DataExpiracao", "EmailEmpresa", "EmpresaNome", "PlanoAtivo", "PlanoId" },
                values: new object[] { new Guid("084498f8-c589-44e9-b804-38b00abca89a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2034, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "VoxDocs@gmail.com", "VoxDocs", true, new Guid("0e8c6b83-49c1-403e-b70c-6fc8e0f09c7f") });

            migrationBuilder.InsertData(
                table: "PastasPrincipais",
                columns: new[] { "Id", "DataCriacao", "EmpresaId", "NomePastaPrincipal" },
                values: new object[] { new Guid("7f8ef49a-8833-4239-8189-2e1947fc8d8b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("084498f8-c589-44e9-b804-38b00abca89a"), "Principal" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_IdNomeEmpresa",
                table: "Documentos",
                column: "IdNomeEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_IdPastaPrincipal",
                table: "Documentos",
                column: "IdPastaPrincipal");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_IdSubPasta",
                table: "Documentos",
                column: "IdSubPasta");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasContratante_PlanoId",
                table: "EmpresasContratante",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_PastasPrincipais_EmpresaId",
                table: "PastasPrincipais",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPastas_PastaPrincipalId",
                table: "SubPastas",
                column: "PastaPrincipalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Pagamento");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SubPastas");

            migrationBuilder.DropTable(
                name: "PastasPrincipais");

            migrationBuilder.DropTable(
                name: "EmpresasContratante");

            migrationBuilder.DropTable(
                name: "PlanosVoxDocs");
        }
    }
}
