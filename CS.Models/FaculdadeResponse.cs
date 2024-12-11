using static Faculdade;

namespace CS.Models
{
    public class FaculdadeResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CNPJ { get; set; }
        public string Telefone { get; set; }
        public string EmailResponsavel { get; set; }
        public EnderecoRequest Endereco { get; set; }
        public Faculdade.TipoFacul Tipo { get; set; }
        public string TipoString => Tipo.ToString();
        public string UniversidadeNome { get; set; }
        public List<CursoResponse> Cursos { get; set; } = new List<CursoResponse>();
    }
}