using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase
    {
        private readonly ProjetoDbContext _context;

        public CursoController(ProjetoDbContext context)
        {
            _context = context;
        }

        // Obter todos os cursos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CursoResponse>>> GetCursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Faculdade)
                .Include(c => c.Turmas)
                    .ThenInclude(t => t.Estudantes)
                .Include(c => c.Disciplinas)
                .Select(c => new CursoResponse
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Mensalidade = c.Mensalidade,
                    FaculdadeId = c.FaculdadeId,
                    FaculdadeNome = c.Faculdade.Nome,
                    Turmas = c.Turmas.Select(t => new TurmaResponse
                    {
                        Id = t.Id,
                        Nome = t.Nome,
                        Periodo = t.Periodo,
                        Estudantes = t.Estudantes.Select(e => new EstudanteResponse
                        {
                            Id = e.Id,
                            Nome = e.Nome,
                            NumeroMatricula = e.NumeroMatricula,
                            DataMatricula = e.DataMatricula,
                            TelefonePai = e.TelefonePai,
                            TelefoneMae = e.TelefoneMae
                        }).ToList()
                    }).ToList(),
                    Disciplinas = c.Disciplinas.Select(d => new DisciplinaResponse
                    {
                        Id = d.Id,
                        Nome = d.Nome,
                        Descricao = d.Descricao,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(cursos);
        }

        // Obter um curso pelo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CursoResponse>> GetCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Faculdade)
                .Include(c => c.Turmas)
                .ThenInclude(t => t.Estudantes)
                .Include(c => c.Disciplinas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            var response = new CursoResponse
            {
                Id = curso.Id,
                Nome = curso.Nome,
                Mensalidade = curso.Mensalidade,
                FaculdadeId = curso.FaculdadeId,
                FaculdadeNome = curso.Faculdade.Nome,
                Turmas = curso.Turmas.Select(t => new TurmaResponse
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Periodo = t.Periodo,
                    Estudantes = t.Estudantes.Select(e => new EstudanteResponse
                    {
                        Id = e.Id,
                        Nome = e.Nome,
                        NumeroMatricula = e.NumeroMatricula,
                        DataMatricula = e.DataMatricula,
                        TelefonePai = e.TelefonePai,
                        TelefoneMae = e.TelefoneMae
                    }).ToList()
                }).ToList(),
                Disciplinas = curso.Disciplinas.Select(d => new DisciplinaResponse
                {
                    Id = d.Id,
                    Nome = d.Nome,
                    Descricao = d.Descricao,
                }).ToList()
            };

            return Ok(response);
        }

        // Criar um curso
        [HttpPost]
        public async Task<ActionResult<CursoResponse>> CreateCurso(CursoRequest request)
        {
            var faculdade = await _context.Faculdades.FindAsync(request.FaculdadeId);
            if (faculdade == null)
                return BadRequest("Faculdade não encontrada.");

            var curso = new Curso
            {
                Nome = request.Nome,
                Mensalidade = request.Mensalidade,
                FaculdadeId = request.FaculdadeId
            };

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurso), new { id = curso.Id }, curso);
        }

        // Atualizar um curso
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurso(int id, CursoRequest request)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            curso.Nome = request.Nome;
            curso.Mensalidade = request.Mensalidade;
            curso.FaculdadeId = request.FaculdadeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Deletar um curso
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
