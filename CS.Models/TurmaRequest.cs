namespace CS.Models
{
    public class TurmaRequest
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Turma.PeriodoCurso Periodo { get; set; }
    }
}
