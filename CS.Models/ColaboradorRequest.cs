namespace CS.Models
{
    public class ColaboradorRequest : Pessoa
    {
        public string Cargo { get; set; } = string.Empty;
        public string NumeroRegistro { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; } = DateTime.Now;

        public int? CursoId { get; set; }
    }
}
