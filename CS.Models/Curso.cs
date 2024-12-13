using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace CS.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Mensalidade { get; set; }

        public int FaculdadeId { get; set; }
        [JsonIgnore]
        public Faculdade Faculdade { get; set; }

        [JsonIgnore]
        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();

        [JsonIgnore]
        public ICollection<Disciplina> Disciplinas { get; set; } = new List<Disciplina>();

        public Colaborador? Colaborador { get; set; }
    }
}