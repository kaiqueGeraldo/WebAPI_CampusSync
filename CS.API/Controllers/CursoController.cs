using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace CS.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase
    {
        private readonly ProjetoDbContext _context;
        private readonly ILogger<CursoController> _logger;

        public CursoController(ProjetoDbContext context, ILogger<CursoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Cursos?cpf=12345678900
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CursoResponse>>> GetCursosPorCPF([FromQuery] string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                return BadRequest("O CPF deve ser informado.");
            }

            // Obter o usuário associado ao CPF
            var user = await _context.Users
                .Include(u => u.Faculdades)
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Obtenha os IDs das faculdades associadas ao usuário
            var faculdadesIds = user.Faculdades.Select(f => f.Id).ToList();

            // Obter os cursos relacionados às faculdades associadas ao CPF do usuário
            var cursos = await _context.Cursos
                .Where(c => faculdadesIds.Contains(c.FaculdadeId) && c.Mensalidade > 0)
                .Include(c => c.Faculdade)
                .Include(c => c.Turmas)
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

        // Obter cursos por faculdade com turmas e disciplinas
        [HttpGet("faculdade/{faculdadeId}")]
        public async Task<IActionResult> ObterCursosPorFaculdade(int faculdadeId)
        {
            var cursos = await _context.Cursos
                .Where(c => c.FaculdadeId == faculdadeId)
                .Include(c => c.Turmas)
                .Include(c => c.Disciplinas)
                .ToListAsync();

            if (cursos == null || !cursos.Any())
                return NotFound("Nenhum curso encontrado para a faculdade especificada.");

            var cursosComRelacionamentos = cursos.Select(curso => new
            {
                curso.Id,
                curso.Nome,
                Turmas = curso.Turmas.Select(turma => new
                {
                    turma.Id,
                    turma.Nome,
                    turma.Periodo
                }),
                Disciplinas = curso.Disciplinas.Select(disciplina => new
                {
                    disciplina.Id,
                    disciplina.Nome,
                })
            });

            return Ok(cursosComRelacionamentos);
        }


        // Criar um curso
        [HttpPost]
        public async Task<ActionResult<CursoResponse>> CreateCurso(CursoRequest request)
        {
            var faculdade = await _context.Faculdades.FindAsync(request.FaculdadeId);
            if (faculdade == null)
                return BadRequest("Faculdade não encontrada.");

            var cursoExistente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Nome == request.Nome && c.FaculdadeId == request.FaculdadeId);
            if (cursoExistente != null)
                return BadRequest("Este curso já está cadastrado para esta faculdade.");

            var curso = new Curso
            {
                Nome = request.Nome,
                FaculdadeId = request.FaculdadeId
            };

            if (request.Turmas != null && request.Turmas.Any())
            {
                curso.Turmas = request.Turmas.Select(t => new Turma
                {
                    Nome = t.Nome,
                    Periodo = t.Periodo
                }).ToList();
            }

            if (request.Disciplinas != null && request.Disciplinas.Any())
            {
                curso.Disciplinas = request.Disciplinas.Select(d => new Disciplina
                {
                    Nome = d.Nome,
                    Descricao = d.Descricao
                }).ToList();
            }

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurso), new { id = curso.Id }, new CursoResponse
            {
                Id = curso.Id,
                Nome = curso.Nome,
                FaculdadeId = curso.FaculdadeId,
                FaculdadeNome = faculdade.Nome,
                Turmas = curso.Turmas?.Select(t => new TurmaResponse
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Periodo = t.Periodo
                }).ToList(),
                Disciplinas = curso.Disciplinas?.Select(d => new DisciplinaResponse
                {
                    Id = d.Id,
                    Nome = d.Nome,
                    Descricao = d.Descricao
                }).ToList()
            });
        }

        [HttpPut("atualizar-curso/{id}")]
        public async Task<IActionResult> AtualizarCurso(int id, CursoRequest request)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound("Curso não encontrado.");

            // Atualiza informações básicas
            curso.Nome = request.Nome;
            curso.Mensalidade = request.Mensalidade;
            curso.FaculdadeId = request.FaculdadeId;

            await _context.SaveChangesAsync();
            return Ok(curso);
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

        [HttpPut("{id}/turmas")]
        public async Task<IActionResult> AdicionarTurmas(int id, List<TurmaRequest> turmasRequest)
        {
            var curso = await _context.Cursos
                .Include(c => c.Turmas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound("Curso não encontrado.");

            // Verifica se a adição excederá o limite de 4 turmas
            if (curso.Turmas.Count + turmasRequest.Count > 4)
                return BadRequest("Não é possível adicionar mais de 4 turmas a um curso.");

            var novasTurmas = new List<Turma>();

            foreach (var turmaRequest in turmasRequest)
            {
                // Valida se já existe uma turma com o mesmo período
                if (curso.Turmas.Any(t => t.Periodo == turmaRequest.Periodo) ||
                    novasTurmas.Any(t => t.Periodo == turmaRequest.Periodo))
                {
                    return BadRequest($"Já existe uma turma com o período {turmaRequest.Periodo}.");
                }

                // Adiciona a nova turma
                novasTurmas.Add(new Turma
                {
                    Nome = turmaRequest.Nome,
                    Periodo = turmaRequest.Periodo,
                    CursoId = curso.Id
                });
            }

            // Adiciona todas as novas turmas ao curso
            curso.Turmas.AddRange(novasTurmas);

            await _context.SaveChangesAsync();
            return Ok(curso.Turmas);
        }

        [HttpPut("{id}/disciplinas")]
        public async Task<IActionResult> AdicionarDisciplinas(int id, List<DisciplinaRequest> disciplinasRequest)
        {
            var curso = await _context.Cursos
                .Include(c => c.Disciplinas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound("Curso não encontrado.");

            var novasDisciplinas = new List<Disciplina>();

            foreach (var disciplinaRequest in disciplinasRequest)
            {
                // Valida se já existe uma disciplina com o mesmo nome
                if (curso.Disciplinas.Any(d => d.Nome.Equals(disciplinaRequest.Nome, StringComparison.OrdinalIgnoreCase)) ||
                    novasDisciplinas.Any(d => d.Nome.Equals(disciplinaRequest.Nome, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest($"Já existe uma disciplina com o nome '{disciplinaRequest.Nome}'.");
                }

                // Adiciona a nova disciplina
                novasDisciplinas.Add(new Disciplina
                {
                    Nome = disciplinaRequest.Nome,
                    Descricao = disciplinaRequest.Descricao,
                    CursoId = curso.Id
                });
            }

            // Adiciona todas as novas disciplinas ao curso
            curso.Disciplinas.AddRange(novasDisciplinas);

            await _context.SaveChangesAsync();
            return Ok(curso.Disciplinas);
        }

        // DELETE: api/Curso/{cursoId}/excluir-turma/{turmaId}
        [HttpDelete("{cursoId}/excluir-turma/{turmaId}")]
        public async Task<IActionResult> ExcluirTurma(int cursoId, int turmaId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Turmas)
                    .ThenInclude(t => t.Estudantes)
                .FirstOrDefaultAsync(c => c.Id == cursoId);

            if (curso == null)
                return NotFound(new { message = "Curso não encontrado." });

            var turma = curso.Turmas.FirstOrDefault(t => t.Id == turmaId);
            if (turma == null)
                return NotFound(new { message = "Turma não encontrada." });

            if (turma.Estudantes != null && turma.Estudantes.Any())
            {
                _context.Estudantes.RemoveRange(turma.Estudantes);
            }

            _context.Turmas.Remove(turma);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Turma e estudantes associados excluídos com sucesso.", turmasRestantes = curso.Turmas });
        }

        // DELETE: api/Curso/{cursoId}/excluir-disciplina/{disciplinaId}
        [HttpDelete("{cursoId}/excluir-disciplina/{disciplinaId}")]
        public async Task<IActionResult> ExcluirDisciplina(int cursoId, int disciplinaId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Disciplinas)
                .FirstOrDefaultAsync(c => c.Id == cursoId);

            if (curso == null)
                return NotFound("Curso não encontrado.");

            var disciplina = curso.Disciplinas.FirstOrDefault(d => d.Id == disciplinaId);
            if (disciplina == null)
                return NotFound("Disciplina não encontrada.");

            _context.Disciplinas.Remove(disciplina);
            await _context.SaveChangesAsync();

            return Ok(curso.Disciplinas);
        }

        // DELETE: api/Curso/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            try
            {
                var curso = await _context.Cursos
                    .Include(c => c.Turmas)
                        .ThenInclude(t => t.Estudantes)
                    .Include(c => c.Disciplinas)
                    .Include(c => c.Colaborador)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (curso == null)
                {
                    return NotFound(new { message = "Curso não encontrado." });
                }

                // Exclusão manual de dependências relacionadas
                if (curso.Turmas != null)
                {
                    foreach (var turma in curso.Turmas)
                    {
                        if (turma.Estudantes != null && turma.Estudantes.Any())
                        {
                            _context.Estudantes.RemoveRange(turma.Estudantes);
                        }
                        _context.Turmas.Remove(turma);
                    }
                }

                if (curso.Disciplinas != null && curso.Disciplinas.Any())
                {
                    _context.Disciplinas.RemoveRange(curso.Disciplinas);
                }

                if (curso.Colaborador != null)
                {
                    _context.Colaboradores.Remove(curso.Colaborador);
                }

                // Removendo o curso após excluir dependências
                _context.Cursos.Remove(curso);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir curso: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno do servidor. Tente novamente mais tarde." });
            }
        }
    }

}
