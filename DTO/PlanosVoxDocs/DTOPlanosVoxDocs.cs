// DTOs/DTOPlanosVoxDocs.cs
namespace VoxDocs.DTO
{
    public class DTOPlanosVoxDocs
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; } 
        public required string Periodicidade { get; set; }
        public int ArmazenamentoDisponivel { get; set; }
        public string TokensDisponiveis { get; set; } = "Infinito";
        public int? LimiteAdmin { get; set; }
        public int? LimiteUsuario { get; set; }
    }
}
