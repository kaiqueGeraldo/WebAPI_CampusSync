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
                            CPF = e.CPF,
                            Nome = e.Nome,
                            NumeroMatricula = e.NumeroMatricula,
                            DataMatricula = e.DataMatricula,
                            TelefonePai = e.TelefonePai,
                            TelefoneMae = e.TelefoneMae,
                            Endereco = e.Endereco,
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
                        CPF = e.CPF,
                        Nome = e.Nome,
                        NumeroMatricula = e.NumeroMatricula,
                        DataMatricula = e.DataMatricula,
                        TelefonePai = e.TelefonePai,
                        TelefoneMae = e.TelefoneMae,
                        Endereco = e.Endereco
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

        // Cursos por paginação
        [HttpGet("cursos")]
        public async Task<ActionResult<IEnumerable<CursoResponse>>> GetCursos(
        [FromQuery] string search = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var query = _context.Cursos.AsQueryable();

            // Filtro por nome
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Nome.Contains(search));
            }

            // Paginação
            var totalCursos = await query.CountAsync();
            var cursos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalCursos = totalCursos,
                Cursos = cursos.Select(c => new CursoResponse
                {
                    Id = c.Id,
                    Nome = c.Nome
                }).ToList()
            };

            return Ok(response);
        }

        // cursos por faculdade
        [HttpGet("faculdade/{faculdadeId}")]
        public async Task<IActionResult> ObterCursosPorFaculdade(int faculdadeId)
        {
            var cursos = await _context.Cursos
                .Where(c => c.FaculdadeId == faculdadeId)
                .ToListAsync();

            if (cursos == null || !cursos.Any())
                return NotFound("Nenhum curso encontrado para a faculdade especificada.");

            return Ok(cursos);
        }

        // Criar um curso
        [HttpPost]
        public async Task<ActionResult<CursoResponse>> CreateCurso(CursoRequest request)
        {
            var faculdade = await _context.Faculdades.FindAsync(request.FaculdadeId);
            if (faculdade == null)
                return BadRequest("Faculdade não encontrada.");

            // Verifica se o curso já existe na faculdade
            var cursoExistente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Nome == request.Nome && c.FaculdadeId == request.FaculdadeId);
            if (cursoExistente != null)
                return BadRequest("Este curso já está cadastrado para esta faculdade.");

            var curso = new Curso
            {
                Nome = request.Nome,
                FaculdadeId = request.FaculdadeId
            };

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurso), new { id = curso.Id }, new CursoResponse
            {
                Id = curso.Id,
                Nome = curso.Nome,
                FaculdadeId = curso.FaculdadeId,
                FaculdadeNome = faculdade.Nome
            });
        }

        // Atualizar um curso
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurso(int id, CursoRequest request)
        {
            var curso = await _context.Cursos
                .Include(c => c.Turmas)
                .Include(c => c.Disciplinas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            curso.Mensalidade = request.Mensalidade;

            // Valida a quantidade de turmas
            if (request.Turmas.Count != request.QuantidadeTurmas)
                return BadRequest("A quantidade de turmas fornecida não corresponde à quantidade especificada.");

            // Verifica duplicidade de períodos
            var periodosDuplicados = request.Turmas
                .GroupBy(t => t.Periodo)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (periodosDuplicados.Any())
                return BadRequest($"Os seguintes períodos foram repetidos: {string.Join(", ", periodosDuplicados)}.");

            // Atualiza as turmas
            var turmasExistentes = curso.Turmas.ToList();
            foreach (var turmaRequest in request.Turmas)
            {
                var turma = turmasExistentes.FirstOrDefault(t => t.Id == turmaRequest.Id);
                if (turma != null)
                {
                    turma.Nome = turmaRequest.Nome;
                    turma.Periodo = turmaRequest.Periodo;
                }
                else
                {
                    curso.Turmas.Add(new Turma
                    {
                        Nome = turmaRequest.Nome,
                        Periodo = turmaRequest.Periodo,
                        CursoId = curso.Id
                    });
                }
            }

            foreach (var turma in turmasExistentes)
            {
                if (!request.Turmas.Any(t => t.Id == turma.Id))
                {
                    _context.Turmas.Remove(turma);
                }
            }

            // Atualiza as disciplinas
            var disciplinasExistentes = curso.Disciplinas.ToList();
            foreach (var disciplinaRequest in request.Disciplinas)
            {
                var disciplina = disciplinasExistentes.FirstOrDefault(d => d.Id == disciplinaRequest.Id);
                if (disciplina != null)
                {
                    disciplina.Nome = disciplinaRequest.Nome;
                    disciplina.Descricao = disciplinaRequest.Descricao;
                }
                else
                {
                    curso.Disciplinas.Add(new Disciplina
                    {
                        Nome = disciplinaRequest.Nome,
                        Descricao = disciplinaRequest.Descricao,
                        CursoId = curso.Id
                    });
                }
            }

            foreach (var disciplina in disciplinasExistentes)
            {
                if (!request.Disciplinas.Any(d => d.Id == disciplina.Id))
                {
                    _context.Disciplinas.Remove(disciplina);
                }
            }

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
