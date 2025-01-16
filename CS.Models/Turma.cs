using System.Text.Json.Serialization;

namespace CS.Models
{
    public class Turma
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public PeriodoCurso Periodo { get; set; }

        public int CursoId { get; set; }
        [JsonIgnore]
        public Curso Curso { get; set; }
        public ICollection<Estudante> Estudantes { get; set; } = new List<Estudante>();
        public enum PeriodoCurso
        {
            Matutino,
            Vespertino,
            Noturno,
            Integral
        }
    }
}
