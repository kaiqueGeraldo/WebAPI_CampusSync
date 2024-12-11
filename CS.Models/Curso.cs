using Microsoft.EntityFrameworkCore;

namespace CS.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Mensalidade { get; set; }

        public int FaculdadeId { get; set; }
        public Faculdade Faculdade { get; set; }

        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();

        public ICollection<Disciplina> Disciplinas { get; set; } = new List<Disciplina>();

        public Colaborador? Colaborador { get; set; }
    }
}