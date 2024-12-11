namespace CS.Models
{
    public class EstudanteResponse : Pessoa
    {
        public int Id { get; set; }
        public string NumeroMatricula { get; set; } = string.Empty;
        public DateTime DataMatricula { get; set; }
        public string TelefonePai { get; set; } = string.Empty;
        public string TelefoneMae { get; set; } = string.Empty;
        public string TurmaNome { get; set; } = string.Empty;
    }
}
