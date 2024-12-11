namespace CS.Models
{
    public class ColaboradorResponse : Pessoa
    {
        public int Id { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public string NumeroRegistro { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; }

        public int? CursoId { get; set; }
        public string? CursoNome { get; set; }
    }
}
