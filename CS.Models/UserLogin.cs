using System.ComponentModel.DataAnnotations;

namespace CS.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Email é obrigatório.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }
}
