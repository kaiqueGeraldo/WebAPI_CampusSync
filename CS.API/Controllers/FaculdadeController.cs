using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class FaculdadeController : ControllerBase
{
    private readonly ProjetoDbContext _context;

    public FaculdadeController(ProjetoDbContext context)
    {
        _context = context;
    }

    // GET: api/Faculdade?cpf=12345678900
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaculdadeResponse>>> GetFaculdades([FromQuery] string cpf)
    {
        if (string.IsNullOrEmpty(cpf))
        {
            return BadRequest("O CPF deve ser informado.");
        }

        var faculdades = await _context.Faculdades
            .Include(f => f.User)
            .Include(f => f.Cursos)
            .Include(f => f.Endereco)
            .Where(f => f.UserCPF == cpf)
            .Select(f => MapFaculdadeToResponse(f))
            .ToListAsync();

        if (!faculdades.Any())
        {
            return NotFound("Nenhuma faculdade encontrada para o CPF informado.");
        }

        return Ok(faculdades);
    }

    // GET: api/Faculdade/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FaculdadeResponse>> GetFaculdade(int id)
    {
        var faculdade = await _context.Faculdades
            .Include(f => f.User)
            .Include(f => f.Cursos)
            .Include(f => f.Endereco)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (faculdade == null)
        {
            return NotFound();
        }

        return Ok(MapFaculdadeToResponse(faculdade));
    }

    // POST: api/Faculdade
    [HttpPost]
    public async Task<ActionResult<FaculdadeResponse>> CriarFaculdade(FaculdadeRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Faculdades)
            .FirstOrDefaultAsync(u => u.CPF == request.UserCPF);

        if (user == null)
        {
            return BadRequest("Usuário não encontrado.");
        }

        var existingFaculdade = await _context.Faculdades
            .FirstOrDefaultAsync(f => f.UserCPF == request.UserCPF && f.CNPJ == request.CNPJ);

        if (existingFaculdade != null)
        {
            return BadRequest("Usuário já tem uma faculdade com este CNPJ.");
        }

        var faculdade = new Faculdade
        {
            Nome = request.Nome,
            CNPJ = request.CNPJ,
            Telefone = request.Telefone,
            EmailResponsavel = request.EmailResponsavel,
            Endereco = new Endereco
            {
                Logradouro = request.Endereco.Logradouro,
                Numero = request.Endereco.Numero,
                Bairro = request.Endereco.Bairro,
                Cidade = request.Endereco.Cidade,
                Estado = request.Endereco.Estado,
                CEP = request.Endereco.CEP
            },
            Tipo = request.Tipo,
            UserCPF = user.CPF
        };

        _context.Faculdades.Add(faculdade);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaculdade), new { id = faculdade.Id }, MapFaculdadeToResponse(faculdade));
    }

    // PUT: api/Faculdade/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarFaculdade(int id, FaculdadeRequest request)
    {
        var faculdade = await _context.Faculdades.Include(f => f.User).FirstOrDefaultAsync(f => f.Id == id);

        if (faculdade == null)
        {
            return NotFound();
        }

        if (faculdade.User == null)
        {
            return BadRequest("Faculdade não está associada a um usuário válido.");
        }

        faculdade.Nome = request.Nome;
        faculdade.CNPJ = request.CNPJ;
        faculdade.Telefone = request.Telefone;
        faculdade.EmailResponsavel = request.EmailResponsavel;
        faculdade.Endereco.Logradouro = request.Endereco.Logradouro;
        faculdade.Endereco.Numero = request.Endereco.Numero;
        faculdade.Endereco.Bairro = request.Endereco.Bairro;
        faculdade.Endereco.Cidade = request.Endereco.Cidade;
        faculdade.Endereco.Estado = request.Endereco.Estado;
        faculdade.Endereco.CEP = request.Endereco.CEP;
        faculdade.Tipo = request.Tipo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Faculdade/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarFaculdade(int id)
    {
        var faculdade = await _context.Faculdades.FindAsync(id);

        if (faculdade == null)
        {
            return NotFound();
        }

        _context.Faculdades.Remove(faculdade);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static FaculdadeResponse MapFaculdadeToResponse(Faculdade faculdade)
    {
        return new FaculdadeResponse
        {
            Id = faculdade.Id,
            Nome = faculdade.Nome,
            CNPJ = faculdade.CNPJ,
            Telefone = faculdade.Telefone,
            EmailResponsavel = faculdade.EmailResponsavel,
            Endereco = new EnderecoRequest
            {
                Logradouro = faculdade.Endereco.Logradouro,
                Numero = faculdade.Endereco.Numero,
                Bairro = faculdade.Endereco.Bairro,
                Cidade = faculdade.Endereco.Cidade,
                Estado = faculdade.Endereco.Estado,
                CEP = faculdade.Endereco.CEP
            },
            Tipo = faculdade.Tipo,
            UniversidadeNome = faculdade.User?.UniversidadeNome ?? "Universidade não definida",
            Cursos = faculdade.Cursos.Select(c => new CursoResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Mensalidade = c.Mensalidade, 
                FaculdadeId = c.FaculdadeId,
                FaculdadeNome = c.Faculdade.Nome,
                ColaboradorNome = c.Colaborador?.Nome,
            }).ToList()
        };
    }
}
