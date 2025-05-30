namespace VoxDocs.DTO
{
    public class DTOUser
    {
        public required string Usuario { get; set; }
        public required string Email { get; set; }
        public required string Senha { get; set; }
        public required string PermissionAccount { get; set; }
        public string? EmpresaContratante { get; set; }
        public string? PlanoPago { get; set; }
        public string? LimiteUsuario { get; internal set; }
        public string? LimiteAdmin { get; internal set; }
    }

    public class UsuarioTotalEmpresa
    {
        public string? NomeEmpresa { get; set; }
        public int TotalUsuarios { get; set; }

    }

    public class DTOUserLogin
    {
        public required string Usuario { get; set; }
        public required string Senha { get; set; }
    }

    public class ErrorResponse
    {
        public required string Mensagem { get; set; }
        public required string Detalhes { get; set; }
    }

    public class UserInfoDTO
    {
        public required string Usuario { get; init; }
        public required string Email { get; init; }
        public required string PermissionAccount { get; init; }
        public string? EmpresaContratante { get; init; }
    }

    public class DTOUserLoginPasswordChange
    {
        public required string Usuario { get; set; }
        public required string SenhaAntiga { get; set; }
        public required string NovaSenha { get; set; }
    }

    public class DTOResetPasswordWithToken
    {
        public string? Token { get; set; }
        public string? NovaSenha { get; set; }
    }
}