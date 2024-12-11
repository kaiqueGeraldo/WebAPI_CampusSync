namespace CS.Models
{
    public class EstudanteRequest : Pessoa
    {
        public string NumeroMatricula { get; set; } = string.Empty;
        public DateTime DataMatricula { get; set; } = DateTime.Now;
        public string TelefonePai { get; set; } = string.Empty;
        public string TelefoneMae { get; set; } = string.Empty;
        public int TurmaId { get; set; }
    }
}
