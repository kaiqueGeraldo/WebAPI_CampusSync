using System.Text.Json.Serialization;

namespace CS.Models
{
    public class Disciplina
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int CursoId { get; set; }
        [JsonIgnore]
        public Curso Curso { get; set; }
    }
}
