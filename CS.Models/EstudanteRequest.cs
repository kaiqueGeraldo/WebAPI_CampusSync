namespace CS.Models
{
    public class EstudanteRequest
    {
        public int Id { get; set; }
        public string? Nome { get; set; } = string.Empty;
        public string? CPF { get; set; } = string.Empty;
        public string? RG { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? NumeroMatricula { get; set; } = string.Empty;
        public DateTime DataMatricula { get; set; } = DateTime.Now;
        public string? Telefone { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string? TituloEleitor { get; set; } = string.Empty;
        public string? EstadoCivil { get; set; } = string.Empty;
        public string? Nacionalidade { get; set; } = string.Empty;
        public string? CorRacaEtnia { get; set; } = string.Empty;
        public string? Escolaridade { get; set; } = string.Empty;
        public string? NomePai { get; set; } = string.Empty;
        public string? NomeMae { get; set; } = string.Empty;
        public Endereco Endereco { get; set; }
        public int TurmaId { get; set; }
    }
}
