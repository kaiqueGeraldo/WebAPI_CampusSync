using CS.Models;
using Microsoft.EntityFrameworkCore;

public class CursoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Mensalidade { get; set; }

    public int FaculdadeId { get; set; }
    public string FaculdadeNome { get; set; } = string.Empty;
    public string? ColaboradorNome { get; set; }
    public ICollection<TurmaResponse> Turmas { get; set; } = new List<TurmaResponse>();
    public ICollection<DisciplinaResponse> Disciplinas { get; set; } = new List<DisciplinaResponse>();
}

