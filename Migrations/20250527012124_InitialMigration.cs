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
                name: "EmpresasContratantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaContratante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasContratantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosCartao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartaoNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidadeCartao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CvvCartao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCartao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoPlano = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorPlano = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosCartao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosPix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QRCodePix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoPlano = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorPlano = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Confirmado = table.Column<bool>(type: "bit", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosPix", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastaPrincipal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomePastaPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpresaContratante = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastaPrincipal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanosVoxDocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Periodicidade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArmazenamentoDisponivel = table.Column<int>(type: "int", nullable: false),
                    TokensDisponiveis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LimiteAdmin = table.Column<int>(type: "int", nullable: true),
                    LimiteUsuario = table.Column<int>(type: "int", nullable: true),
                    MaxStorageGb = table.Column<int>(type: "int", nullable: false),
                    UserLimit = table.Column<int>(type: "int", nullable: false),
                    TokenLimit = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosVoxDocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PermissionAccount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpresaContratante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanoPago = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LimiteUsuario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LimiteAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubPastas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeSubPasta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomePastaPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpresaContratante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PastaPrincipalModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPastas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubPastas_PastaPrincipal_PastaPrincipalModelId",
                        column: x => x.PastaPrincipalModelId,
                        principalTable: "PastaPrincipal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeArquivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlArquivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioCriador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioUltimaAlteracao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataUltimaAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Empresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomePastaPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomeSubPasta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanhoArquivo = table.Column<long>(type: "bigint", nullable: false),
                    NivelSeguranca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContadorAcessos = table.Column<int>(type: "int", nullable: false),
                    TokenSeguranca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descrição = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubPastaModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_SubPastas_SubPastaModelId",
                        column: x => x.SubPastaModelId,
                        principalTable: "SubPastas",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PlanosVoxDocs",
                columns: new[] { "Id", "ArmazenamentoDisponivel", "Description", "Duration", "LimiteAdmin", "LimiteUsuario", "MaxStorageGb", "Name", "Periodicidade", "Price", "TokenLimit", "TokensDisponiveis", "UserLimit" },
                values: new object[,]
                {
                    { 1, 50, "Arquivamento de Dados Com Segurança", 1, 1, 5, 0, "Plano Básico Mensal", "Mensal", 9.99m, null, "Infinito", 0 },
                    { 2, 50, "Sistema de Criptografia de Ponta e Comando de Voz", 1, 2, 10, 0, "Plano Intermediário Mensal", "Mensal", 19.99m, null, "Infinito", 0 },
                    { 3, 50, "Todas Funcionalidades", 1, 5, 20, 0, "Plano Avançado Mensal", "Mensal", 29.99m, null, "Infinito", 0 },
                    { 4, 100, "Arquivamento de Dados Com Segurança", 3, 1, 5, 0, "Plano Básico Trimestral", "Trimestral", 27.99m, null, "Infinito", 0 },
                    { 5, 100, "Sistema de Criptografia de Ponta e Comando de Voz", 3, 2, 10, 0, "Plano Intermediário Trimestral", "Trimestral", 54.99m, null, "Infinito", 0 },
                    { 6, 100, "Todas Funcionalidades", 3, 5, 20, 0, "Plano Avançado Trimestral", "Trimestral", 79.99m, null, "Infinito", 0 },
                    { 7, 150, "Arquivamento de Dados Com Segurança", 6, 1, 5, 0, "Plano Básico Semestral", "Semestral", 49.99m, null, "Infinito", 0 },
                    { 8, 150, "Sistema de Criptografia de Ponta e Comando de Voz", 6, 2, 10, 0, "Plano Intermediário Semestral", "Semestral", 99.99m, null, "Infinito", 0 },
                    { 9, 150, "Todas Funcionalidades", 6, 5, 20, 0, "Plano Avançado Semestral", "Semestral", 149.99m, null, "Infinito", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SubPastaModelId",
                table: "Documentos",
                column: "SubPastaModelId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPastas_PastaPrincipalModelId",
                table: "SubPastas",
                column: "PastaPrincipalModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "EmpresasContratantes");

            migrationBuilder.DropTable(
                name: "PagamentosCartao");

            migrationBuilder.DropTable(
                name: "PagamentosPix");

            migrationBuilder.DropTable(
                name: "PlanosVoxDocs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SubPastas");

            migrationBuilder.DropTable(
                name: "PastaPrincipal");
        }
    }
}
