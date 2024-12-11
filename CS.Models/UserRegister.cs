namespace CS.Models
{
    public class UserRegister
    {
        public string CPF { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? UrlImagem { get; set; }
        public string? UniversidadeNome { get; set; }
        public string? UniversidadeCNPJ { get; set; }
        public string? UniversidadeContatoInfo { get; set; }
    }
}
