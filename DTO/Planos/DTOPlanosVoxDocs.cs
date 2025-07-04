using System;
using System.ComponentModel.DataAnnotations;

namespace VoxDocs.DTO
{
    public class DTOPlanosVoxDocs
    {

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicidade { get; set; }

        [Range(1, 36)]
        public int? Duracao { get; set; }

        [Range(1, 9999)]
        public int? ArmazenamentoDisponivel { get; set; }

        [Range(1, 100)]
        public int? LimiteAdmin { get; set; }

        [Range(1, 1000)]
        public int? LimiteUsuario { get; set; }
    }

    public class DTOSelecaoPlano
    {
        [Required]
        public Guid PlanoId { get; set; }

        [Required]
        [StringLength(100)]
        public string NomePlano { get; set; }

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicidade { get; set; }

        [Range(1, 36)]
        public int? Duracao { get; set; }

        [Range(1, 9999)]
        public int? Armazenamento { get; set; }

        [Range(1, 100)]
        public int? LimiteAdmin { get; set; }

        [Range(1, 1000)]
        public int? LimiteUsuario { get; set; }

        [Required]
        public DateTime DataValidade { get; set; }

        [Required]
        [MinLength(1)]
        public string[] Caracteristicas { get; set; }
    }

    public class PlanoSelecionadoDTO
    {
        [Required(ErrorMessage = "ID do plano é obrigatório")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Nome do plano é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do plano não pode exceder 100 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Descrição do plano é obrigatória")]
        [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Preço do plano é obrigatório")]
        [Range(0, 999999.99, ErrorMessage = "Preço deve estar entre 0 e 999999.99")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "Periodicidade é obrigatória")]
        [StringLength(20, ErrorMessage = "Periodicidade não pode exceder 20 caracteres")]
        public string Periodicidade { get; set; }

        [Range(1, 36, ErrorMessage = "Duração deve estar entre 1 e 36 meses")]
        public int? Duracao { get; set; }

        [Range(1, 9999, ErrorMessage = "Armazenamento deve estar entre 1 e 9999 GB")]
        public int? Armazenamento { get; set; }

        [Range(1, 100, ErrorMessage = "Limite de administradores deve estar entre 1 e 100")]
        public int? LimiteAdmin { get; set; }

        [Range(1, 1000, ErrorMessage = "Limite de usuários deve estar entre 1 e 1000")]
        public int? LimiteUsuario { get; set; }

        [Required(ErrorMessage = "Data de validade é obrigatória")]
        public DateTime DataValidade { get; set; }

        [Required(ErrorMessage = "Características são obrigatórias")]
        [MinLength(1, ErrorMessage = "Deve haver pelo menos uma característica")]
        public string[] Caracteristicas { get; set; }
    }

    public enum StatusPlano
    {
        Ativo,
        Inativo,
        Cancelado
    }
}