using CS.Models;

public class Faculdade
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string EmailResponsavel { get; set; } = string.Empty;
    public Endereco Endereco { get; set; } = new Endereco();
    public TipoFacul Tipo { get; set; }

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    public string UserCPF { get; set; } = string.Empty;
    public User? User { get; set; } = new User();

    public enum TipoFacul
    {
        Publica,
        Privada,
        Militar
    }
}
