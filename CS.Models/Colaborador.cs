namespace CS.Models
{
    public class Colaborador
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }
        public Pessoa Pessoa { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public string NumeroRegistro { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; } = DateTime.Now;

        public int? CursoId { get; set; }
        public Curso? Curso { get; set; }
        public string UserCPFColaboradores { get; set; } = string.Empty;
        public User User { get; set; }
    }
}
