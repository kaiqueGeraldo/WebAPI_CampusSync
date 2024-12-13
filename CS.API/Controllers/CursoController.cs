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

        // Criar um curso
        [HttpPost]
        public async Task<ActionResult<CursoResponse>> CreateCurso(CursoRequest request)
        {
            var faculdade = await _context.Faculdades.FindAsync(request.FaculdadeId);
            if (faculdade == null)
                return BadRequest("Faculdade não encontrada.");

            if (request.Turmas.Count != request.QuantidadeTurmas)
                return BadRequest("A quantidade de turmas fornecida não corresponde à quantidade especificada.");

            var periodosDuplicados = request.Turmas
                .GroupBy(t => t.Periodo)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (periodosDuplicados.Any())
                return BadRequest($"Os seguintes períodos foram repetidos: {string.Join(", ", periodosDuplicados)}.");

            var curso = new Curso
            {
                Nome = request.Nome,
                Mensalidade = request.Mensalidade,
                FaculdadeId = request.FaculdadeId
            };

            // Adiciona as turmas associadas
            foreach (var turmaRequest in request.Turmas)
            {
                var turma = new Turma
                {
                    Nome = turmaRequest.Nome,
                    Periodo = turmaRequest.Periodo,
                    Curso = curso
                };
                curso.Turmas.Add(turma);
            }

            // Adiciona as disciplinas associadas
            foreach (var disciplinaRequest in request.Disciplinas)
            {
                var disciplina = new Disciplina
                {
                    Nome = disciplinaRequest.Nome,
                    Descricao = disciplinaRequest.Descricao,
                    Curso = curso
                };
                curso.Disciplinas.Add(disciplina);
            }

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            // Inclui as disciplinas ao buscar o curso
            var cursoComFaculdade = await _context.Cursos
                .Include(c => c.Faculdade)
                .Include(c => c.Turmas)
                .Include(c => c.Disciplinas)
                .FirstOrDefaultAsync(c => c.Id == curso.Id);

            if (cursoComFaculdade == null)
                return BadRequest("Curso não encontrado.");

            var response = new CursoResponse
            {
                Id = cursoComFaculdade.Id,
                Nome = cursoComFaculdade.Nome,
                Mensalidade = cursoComFaculdade.Mensalidade,
                FaculdadeId = cursoComFaculdade.FaculdadeId,
                FaculdadeNome = cursoComFaculdade.Faculdade?.Nome,
                Turmas = cursoComFaculdade.Turmas.Select(t => new TurmaResponse
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Periodo = t.Periodo
                }).ToList(),
                Disciplinas = cursoComFaculdade.Disciplinas.Select(d => new DisciplinaResponse
                {
                    Nome = d.Nome,
                    Descricao = d.Descricao
                }).ToList()
            };

            return CreatedAtAction(nameof(GetCurso), new { id = cursoComFaculdade.Id }, response);
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

            curso.Nome = request.Nome;
            curso.Mensalidade = request.Mensalidade;
            curso.FaculdadeId = request.FaculdadeId;

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

            // Salva as alterações
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
