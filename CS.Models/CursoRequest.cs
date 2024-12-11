using Microsoft.EntityFrameworkCore;

namespace CS.Models
{
    public class CursoRequest
    {
        public string Nome { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Mensalidade { get; set; }
        public int FaculdadeId { get; set; }
    }
}
