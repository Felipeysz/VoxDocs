namespace VoxDocs.DTO
{
    public class DTOPastaPrincipal
    {
        public int Id { get; set; }
        public string NomePastaPrincipal { get; set; } = null!;
        public string EmpresaContratante { get; set; } = null!;
        public int Quantidade { get; set; }
    }

    public class DTOSubPasta
    {
        public int Id { get; set; }
        public string NomeSubPasta { get; set; } = null!;
        public string NomePastaPrincipal { get; set; } = null!;
        public string EmpresaContratante { get; set; } = null!;
        public int Quantidade { get; set; }
    }

    public class DTOPastaPrincipalCreate
    {
        public string NomePastaPrincipal { get; set; } = null!;
        public string EmpresaContratante { get; set; } = null!;
    }

    public class DTOSubPastaCreate
    {
        public string NomeSubPasta { get; set; } = null!;
        public string NomePastaPrincipal { get; set; } = null!;
        public string EmpresaContratante { get; set; } = null!;
    }
}