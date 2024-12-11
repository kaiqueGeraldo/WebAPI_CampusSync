namespace CS.Models
{
    public class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string RG { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string TituloEleitor { get; set; } = string.Empty;
        public string EstadoCivil { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string CorRacaEtnia { get; set; } = string.Empty;
        public string Escolaridade { get; set; } = string.Empty;
        public string NomePai { get; set; } = string.Empty;
        public string NomeMae { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public Endereco Endereco { get; set; } = new Endereco();
        public string UrlImagePerfil { get; set; } = string.Empty;
    }
}