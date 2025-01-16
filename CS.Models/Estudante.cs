namespace CS.Models
{
    public class Estudante
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }
        public Pessoa Pessoa { get; set; }
        public string NumeroMatricula { get; set; } = string.Empty;
        public DateTime DataMatricula { get; set; } = DateTime.Now;

        public int TurmaId { get; set; }
        public Turma Turma { get; set; }
    }
}