using static Faculdade;

namespace CS.Models
{
    public class FaculdadeRequest
    {
        public string? Nome { get; set; }
        public string? CNPJ { get; set; }
        public string? Telefone { get; set; }
        public string? EmailResponsavel { get; set; }
        public EnderecoRequest? Endereco { get; set; }
        public Faculdade.TipoFacul Tipo { get; set; }
        public List<string>? CursosOferecidos { get; set; }
        public string UserCPF { get; set; }
    }
}
