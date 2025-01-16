namespace CS.Models
{
    public class ColaboradorRequest
    {
        public int Id { get; set; }
        public string? Nome { get; set; } = string.Empty;
        public string? CPF { get; set; } = string.Empty;
        public string? RG { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Cargo { get; set; } = string.Empty;
        public string? NumeroRegistro { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; } = DateTime.Now;
        public string? Telefone { get; set; } = string.Empty;
        public string? TituloEleitor { get; set; } = string.Empty;
        public string? EstadoCivil { get; set; } = string.Empty;
        public string? Nacionalidade { get; set; } = string.Empty;
        public string? CorRacaEtnia { get; set; } = string.Empty;
        public string? Escolaridade { get; set; } = string.Empty;
        public string? NomePai { get; set; } = string.Empty;
        public string? NomeMae { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public Endereco Endereco { get; set; }
        public int? CursoId { get; set; }
        public string UserCPFColaboradores { get; set; }
    }
}
