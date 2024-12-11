using static CS.Models.Turma;

namespace CS.Models
{
    public class TurmaResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public PeriodoCurso Periodo { get; set; }
        public ICollection<EstudanteResponse> Estudantes { get; set; } = new List<EstudanteResponse>();
    }

}
