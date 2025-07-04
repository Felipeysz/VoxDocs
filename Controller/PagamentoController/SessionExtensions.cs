using System.Text.Json;
using VoxDocs.DTO;
using VoxDocs.PagamentoViewModel;

namespace VoxDocs.Controllers
{
    public static class SessionExtensions
    {
        private const string PlanoIdKey = "Plano_Id";
        private const string PlanoNomeKey = "Plano_Nome";
        private const string PlanoPrecoKey = "Plano_Preco";
        private const string EmpresaNomeKey = "Empresa_Nome";
        private const string EmpresaEmailKey = "Empresa_Email";
        private const string PastasPrincipaisKey = "Pastas_Principais";
        private const string SubPastasKey = "SubPastas";
        private const string UsuariosKey = "Usuarios";

        public static void SetPlano(this ISession session, Guid id, string nome, decimal preco)
        {
            session.SetString(PlanoIdKey, id.ToString());
            session.SetString(PlanoNomeKey, nome);
            session.SetString(PlanoPrecoKey, preco.ToString());
        }

        public static bool HasPlano(this ISession session) =>
            !string.IsNullOrEmpty(session.GetString(PlanoIdKey));

        public static Guid GetPlanoId(this ISession session) =>
            Guid.TryParse(session.GetString(PlanoIdKey), out var id) ? id : Guid.Empty;

        public static string GetPlanoNome(this ISession session) =>
            session.GetString(PlanoNomeKey) ?? string.Empty;

        public static decimal GetPlanoPreco(this ISession session) =>
            decimal.TryParse(session.GetString(PlanoPrecoKey), out var preco) ? preco : 0;

        public static void SetEmpresa(this ISession session, string nome, string email)
        {
            session.SetString(EmpresaNomeKey, nome);
            session.SetString(EmpresaEmailKey, email);
        }

        public static bool HasEmpresa(this ISession session) =>
            !string.IsNullOrEmpty(session.GetString(EmpresaNomeKey));

        public static string GetEmpresaNome(this ISession session) =>
            session.GetString(EmpresaNomeKey) ?? string.Empty;

        public static string GetEmpresaEmail(this ISession session) =>
            session.GetString(EmpresaEmailKey) ?? string.Empty;

        public static void SetPastasPrincipais(this ISession session, List<PastaPrincipalViewModel> pastas)
        {
            session.SetString(PastasPrincipaisKey, JsonSerializer.Serialize(pastas));
        }

        public static List<PastaPrincipalViewModel> GetPastasPrincipais(this ISession session)
        {
            var json = session.GetString(PastasPrincipaisKey);
            return string.IsNullOrEmpty(json)
                ? new List<PastaPrincipalViewModel>()
                : JsonSerializer.Deserialize<List<PastaPrincipalViewModel>>(json);
        }

        public static void SetSubPastas(this ISession session, List<SubPastaViewModel> subPastas)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            session.SetString(SubPastasKey, JsonSerializer.Serialize(subPastas, options));
        }

        public static List<SubPastaViewModel> GetSubPastas(this ISession session)
        {
            var json = session.GetString(SubPastasKey);
            if (string.IsNullOrEmpty(json))
                return new List<SubPastaViewModel>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<List<SubPastaViewModel>>(json, options) ?? new List<SubPastaViewModel>();
        }

        public static void SetUsuarios(this ISession session, List<PagamentoUsuarioDTO> usuarios)
        {
            session.SetString(UsuariosKey, JsonSerializer.Serialize(usuarios));
        }

        public static List<PagamentoUsuarioDTO> GetUsuarios(this ISession session)
        {
            var json = session.GetString(UsuariosKey);
            return string.IsNullOrEmpty(json)
                ? new List<PagamentoUsuarioDTO>()
                : JsonSerializer.Deserialize<List<PagamentoUsuarioDTO>>(json);
        }
    }
}