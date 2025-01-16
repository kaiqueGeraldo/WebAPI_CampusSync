using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

[ApiController]
[Route("api/[controller]")]
public class FaculdadeController : ControllerBase
{
    private readonly ProjetoDbContext _context;
    private readonly ILogger<FaculdadeController> _logger;

    public FaculdadeController(ProjetoDbContext context, ILogger<FaculdadeController> logger)
    {
        _context = context;
        _logger = logger;
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
            .Where(f => f.UserCPF == cpf)
            .Select(f => new FaculdadeResponse
            {
                Id = f.Id,
                Nome = f.Nome,
                CNPJ = f.CNPJ,
                Telefone = f.Telefone,
                EmailResponsavel = f.EmailResponsavel,
                Tipo = f.Tipo,
                Endereco = new EnderecoRequest
                {
                    Logradouro = f.Endereco.Logradouro,
                    Numero = f.Endereco.Numero,
                    Bairro = f.Endereco.Bairro,
                    Cidade = f.Endereco.Cidade,
                    Estado = f.Endereco.Estado,
                    CEP = f.Endereco.CEP
                },
                Cursos = f.Cursos.Select(c => new CursoResponse
                {
                    Id = c.Id,
                    Nome = c.Nome
                }).ToList(),
                UniversidadeNome = f.User.UniversidadeNome
            })
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
            UserCPF = user.CPF,
            Cursos = new List<Curso>()
        };

        if (request.CursosOferecidos != null && request.CursosOferecidos.Any())
        {
            var cursos = request.CursosOferecidos.Select(nome => new Curso
            {
                Nome = nome,
            }).ToList();

            faculdade.Cursos = cursos;
        }

        _context.Faculdades.Add(faculdade);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaculdade), new { id = faculdade.Id }, MapFaculdadeToResponse(faculdade));
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarFaculdade(int id, FaculdadeRequest request)
    {
        var faculdade = await _context.Faculdades.Include(f => f.Endereco).FirstOrDefaultAsync(f => f.Id == id);

        if (faculdade == null)
        {
            return NotFound("Faculdade não encontrada.");
        }

        // Verificar se os valores foram passados no request e, se sim, atualizar
        if (!string.IsNullOrWhiteSpace(request.Nome))
            faculdade.Nome = request.Nome;

        if (!string.IsNullOrWhiteSpace(request.CNPJ))
            faculdade.CNPJ = request.CNPJ;

        if (!string.IsNullOrWhiteSpace(request.Telefone))
            faculdade.Telefone = request.Telefone;

        if (!string.IsNullOrWhiteSpace(request.EmailResponsavel))
            faculdade.EmailResponsavel = request.EmailResponsavel;

        if (request.Tipo != null)
            faculdade.Tipo = request.Tipo;

        // Se o endereço for passado, atualize apenas os campos que são fornecidos
        if (request.Endereco != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Endereco.Logradouro))
                faculdade.Endereco.Logradouro = request.Endereco.Logradouro;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Numero))
                faculdade.Endereco.Numero = request.Endereco.Numero;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Bairro))
                faculdade.Endereco.Bairro = request.Endereco.Bairro;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Cidade))
                faculdade.Endereco.Cidade = request.Endereco.Cidade;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Estado))
                faculdade.Endereco.Estado = request.Endereco.Estado;

            if (!string.IsNullOrWhiteSpace(request.Endereco.CEP))
                faculdade.Endereco.CEP = request.Endereco.CEP;
        }

        if (!string.IsNullOrWhiteSpace(request.UserCPF))
            faculdade.UserCPF = request.UserCPF;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("adicionar-cursos/{faculdadeId}")]
    public async Task<IActionResult> AdicionarCursos(int faculdadeId, [FromBody] AdicionarCursosRequest request)
    {
        // Busca a faculdade com seus cursos
        var faculdade = await _context.Faculdades
            .Include(f => f.Cursos)
            .FirstOrDefaultAsync(f => f.Id == faculdadeId);

        if (faculdade == null)
            return NotFound("Faculdade não encontrada.");

        // Identifica cursos que já existem na faculdade
        var nomesCursosExistentes = faculdade.Cursos.Select(c => c.Nome).ToList();
        var nomesCursosParaAdicionar = request.CursosOferecidos
            .Where(nome => !nomesCursosExistentes.Contains(nome))
            .Distinct()
            .ToList();

        if (!nomesCursosParaAdicionar.Any())
            return BadRequest("Nenhum curso novo para adicionar.");

        var novosCursos = nomesCursosParaAdicionar
            .Select(nome => new Curso
            {
                Nome = nome,
                FaculdadeId = faculdadeId
            })
            .ToList();

        _context.Cursos.AddRange(novosCursos);
        faculdade.Cursos.AddRange(novosCursos);

        await _context.SaveChangesAsync();

        return Ok(faculdade.Cursos);
    }

    // DELETE: api/Faculdade/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaculdade(int id)
    {
        try
        {
            var faculdade = await _context.Faculdades
                .Include(f => f.Cursos)
                    .ThenInclude(c => c.Turmas)
                        .ThenInclude(t => t.Estudantes)
                .Include(f => f.Cursos)
                    .ThenInclude(c => c.Disciplinas)
                .Include(f => f.Cursos)
                    .ThenInclude(c => c.Colaborador)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculdade == null)
            {
                return NotFound(new { message = "Faculdade não encontrada." });
            }

            // Removendo a faculdade e seus relacionamentos em cascata
            _context.Faculdades.Remove(faculdade);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            // Logando o erro
            _logger.LogError($"Erro ao excluir faculdade: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "Erro interno do servidor. Tente novamente mais tarde." });
        }
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
                ColaboradorNome = c.Colaborador?.Pessoa.Nome,
            }).ToList()
        };
    }
}
