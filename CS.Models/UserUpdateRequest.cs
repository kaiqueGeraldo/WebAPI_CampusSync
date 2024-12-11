using System.ComponentModel.DataAnnotations;

namespace CS.Models
{
    public class UserUpdateRequest
    {
        [Required]
        public string CPF { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }

        // Campos para atualização da universidade
        public string UrlImagem { get; set; } = string.Empty;
        public string UniversidadeNome { get; set; } = string.Empty;
        public string UniversidadeCNPJ { get; set; } = string.Empty;
        public string UniversidadeContatoInfo { get; set; } = string.Empty;
    }
}
