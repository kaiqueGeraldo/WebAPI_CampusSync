using System.ComponentModel.DataAnnotations;

namespace CS.Models
{
    public class User
    {
        [Key]
        public string CPF { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }

        // Informações da Universidade
        public string? UrlImagem { get; set; } = string.Empty;
        public string? UniversidadeNome { get; set; } = string.Empty;
        public string? UniversidadeCNPJ { get; set; } = string.Empty;
        public string? UniversidadeContatoInfo { get; set; } = string.Empty;

        public ICollection<Faculdade> Faculdades { get; set; } = new List<Faculdade>();

        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    }
}
