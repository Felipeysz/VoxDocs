namespace VoxDocs.DTO
{
        public class DTOCadastroEmpresa
    {
        public string TokenPix { get; set; } = string.Empty;
        public string EmpresaContratante { get; set; } = string.Empty;
        public string EmailEmpresa { get; set; } = string.Empty;
        public string NomePastaPrincipal { get; set; } = string.Empty;
        public string NomeSubPasta { get; set; } = string.Empty;
        public List<UsuarioCadastro> Usuarios { get; set; } = new List<UsuarioCadastro>();
        public string PlanoPago { get; set; }
    }
        public class UsuarioCadastro
    {
        public string Usuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string PermissionAccount { get; set; } = string.Empty;
    }
    public class DTOEmpresasContratante
    {
        public required string EmpresaContratante { get; set; }
        public required string Email { get; set; }
    }
    public class DTOEmpresasContratantePlano
    {
        public required string EmpresaContratante { get; set; }
        public required string TipoPlano { get; set; }
        public int TotalConsultas { get; set; } // Adicionado
    }
}
