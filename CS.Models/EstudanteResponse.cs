namespace CS.Models
{
    public class EstudanteResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string RG { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string NumeroMatricula { get; set; } = string.Empty;
        public DateTime DataMatricula { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomePai { get; set; } = string.Empty;
        public string NomeMae { get; set; } = string.Empty;
        public Endereco Endereco { get; set; }
        public string TurmaNome { get; set; } = string.Empty;
    }
}
