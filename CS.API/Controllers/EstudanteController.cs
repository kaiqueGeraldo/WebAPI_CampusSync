using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstudanteController : ControllerBase
    {
        private readonly ProjetoDbContext _context;
        private readonly ILogger<EstudanteController> _logger;

        public EstudanteController(ProjetoDbContext context, ILogger<EstudanteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Estudante?cpf=12345678900
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstudanteResponse>>> GetEstudantesPorCPF([FromQuery] string cpf)
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

            // Obter os estudantes relacionados às faculdades associadas ao CPF do usuário
            var estudantes = await _context.Estudantes
                .Include(e => e.Turma)
                .ThenInclude(t => t.Curso)
                .Include(c => c.Pessoa)
                .Where(e => e.Turma != null &&
                            e.Turma.Curso != null &&
                            faculdadesIds.Contains(e.Turma.Curso.FaculdadeId))
                .Select(e => new EstudanteResponse
                {
                    Id = e.Pessoa.Id,
                    Nome = e.Pessoa.Nome,
                    CPF = e.Pessoa.CPF,
                    RG = e.Pessoa.RG,
                    Email = e.Pessoa.Email,
                    Telefone = e.Pessoa.Telefone,
                    NumeroMatricula = e.NumeroMatricula,
                    DataMatricula = e.DataMatricula,
                    DataNascimento = e.Pessoa.DataNascimento,
                    NomePai = e.Pessoa.NomePai,
                    NomeMae = e.Pessoa.NomeMae,
                    Endereco = e.Pessoa.Endereco,
                    TurmaNome = e.Turma != null ? e.Turma.Nome : string.Empty
                })
                .ToListAsync();

            return Ok(estudantes);
        }

        // GET: api/Estudante/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EstudanteResponse>> GetEstudante(int id)
        {
            var estudante = await _context.Estudantes
                .Include(e => e.Turma)
                .Include(c => c.Pessoa)
                .Where(e => e.Id == id)
                .Select(e => new EstudanteResponse
                {
                    Id = e.Id,
                    Nome = e.Pessoa.Nome,
                    CPF = e.Pessoa.CPF,
                    RG = e.Pessoa.RG,
                    Email = e.Pessoa.Email,
                    Telefone = e.Pessoa.Telefone,
                    NumeroMatricula = e.NumeroMatricula,
                    DataMatricula = e.DataMatricula,
                    DataNascimento = e.Pessoa.DataNascimento,
                    NomePai = e.Pessoa.NomePai,
                    NomeMae = e.Pessoa.NomeMae,
                    Endereco = e.Pessoa.Endereco,
                    TurmaNome = e.Turma != null ? e.Turma.Nome : string.Empty
                })
                .FirstOrDefaultAsync();

            if (estudante == null)
            {
                return NotFound();
            }

            return Ok(estudante);
        }

        // POST: api/Estudante
        [HttpPost]
        public async Task<ActionResult<EstudanteResponse>> CreateEstudante(EstudanteRequest estudanteRequest)
        {
            var pessoa = new Pessoa
            {
                Nome = estudanteRequest.Nome,
                CPF = estudanteRequest.CPF,
                RG = estudanteRequest.RG,
                Email = estudanteRequest.Email,
                Telefone = estudanteRequest.Telefone,
                TituloEleitor = estudanteRequest.TituloEleitor,
                EstadoCivil = estudanteRequest.EstadoCivil,
                Nacionalidade = estudanteRequest.Nacionalidade,
                CorRacaEtnia = estudanteRequest.CorRacaEtnia,
                Escolaridade = estudanteRequest.Escolaridade,
                NomePai = estudanteRequest.NomePai,
                NomeMae = estudanteRequest.NomeMae,
                DataNascimento = estudanteRequest.DataNascimento,
                Endereco = new Endereco
                {
                    Logradouro = estudanteRequest.Endereco.Logradouro,
                    Numero = estudanteRequest.Endereco.Numero,
                    Bairro = estudanteRequest.Endereco.Bairro,
                    Cidade = estudanteRequest.Endereco.Cidade,
                    Estado = estudanteRequest.Endereco.Estado,
                    CEP = estudanteRequest.Endereco.CEP
                },
            };

            _context.Pessoas.Add(pessoa);
            await _context.SaveChangesAsync();

            var estudante = new Estudante
            {
                PessoaId = pessoa.Id,
                NumeroMatricula = estudanteRequest.NumeroMatricula,
                DataMatricula = estudanteRequest.DataMatricula,
                TurmaId = estudanteRequest.TurmaId,
            };

            _context.Estudantes.Add(estudante);
            await _context.SaveChangesAsync();

            var estudanteResponse = new EstudanteResponse
            {
                Nome = pessoa.Nome,
                CPF = pessoa.CPF,
                RG = pessoa.RG,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone,
                NumeroMatricula = estudante.NumeroMatricula,
                DataMatricula = estudante.DataMatricula,
                DataNascimento = pessoa.DataNascimento,
                NomePai = pessoa.NomePai,
                NomeMae = pessoa.NomeMae,
                Endereco = pessoa.Endereco,
                TurmaNome = estudante.Turma != null ? estudante.Turma.Nome : string.Empty
            };

            return CreatedAtAction("GetEstudante", new { id = estudante.Id }, estudanteResponse);
        }

        // PUT: api/Estudante/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEstudante(int id, EstudanteRequest estudanteRequest)
        {
            if (id != estudanteRequest.Id)
            {
                return BadRequest();
            }

            var estudante = await _context.Estudantes
                .Include(e => e.Pessoa)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (estudante == null)
            {
                return NotFound();
            }

            // Atualizar apenas os campos não nulos ou não vazios
            if (!string.IsNullOrEmpty(estudanteRequest.Nome))
                estudante.Pessoa.Nome = estudanteRequest.Nome;

            if (!string.IsNullOrEmpty(estudanteRequest.CPF))
                estudante.Pessoa.CPF = estudanteRequest.CPF;

            if (!string.IsNullOrEmpty(estudanteRequest.RG))
                estudante.Pessoa.RG = estudanteRequest.RG;

            if (!string.IsNullOrEmpty(estudanteRequest.Telefone))
                estudante.Pessoa.Telefone = estudanteRequest.Telefone;

            if (!string.IsNullOrEmpty(estudanteRequest.TituloEleitor))
                estudante.Pessoa.TituloEleitor = estudanteRequest.TituloEleitor;

            if (!string.IsNullOrEmpty(estudanteRequest.EstadoCivil))
                estudante.Pessoa.EstadoCivil = estudanteRequest.EstadoCivil;

            if (!string.IsNullOrEmpty(estudanteRequest.Nacionalidade))
                estudante.Pessoa.Nacionalidade = estudanteRequest.Nacionalidade;

            if (!string.IsNullOrEmpty(estudanteRequest.CorRacaEtnia))
                estudante.Pessoa.CorRacaEtnia = estudanteRequest.CorRacaEtnia;

            if (!string.IsNullOrEmpty(estudanteRequest.Escolaridade))
                estudante.Pessoa.Escolaridade = estudanteRequest.Escolaridade;

            if (!string.IsNullOrEmpty(estudanteRequest.NomePai))
                estudante.Pessoa.NomePai = estudanteRequest.NomePai;

            if (!string.IsNullOrEmpty(estudanteRequest.NomeMae))
                estudante.Pessoa.NomeMae = estudanteRequest.NomeMae;

            if (estudanteRequest.DataNascimento != null)
                estudante.Pessoa.DataNascimento = estudanteRequest.DataNascimento;

            if (!string.IsNullOrEmpty(estudanteRequest.NumeroMatricula))
                estudante.NumeroMatricula = estudanteRequest.NumeroMatricula;

            if (estudanteRequest.DataMatricula != null)
                estudante.DataMatricula = estudanteRequest.DataMatricula;

            if (estudanteRequest.Endereco != null)
                estudante.Pessoa.Endereco = estudanteRequest.Endereco;

            if (estudanteRequest.TurmaId != null)
                estudante.TurmaId = estudanteRequest.TurmaId;

            // Marca o estado da entidade como modificado
            _context.Entry(estudante).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Estudante/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstudante(int id)
        {
            try
            {
                var estudante = await _context.Estudantes
                    .Include(e => e.Pessoa)
                    .Include(e => e.Turma)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (estudante == null)
                {
                    return NotFound(new { message = "Estudante não encontrado." });
                }

                // Remove o estudante e suas relações associadas
                _context.Estudantes.Remove(estudante);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir estudante: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno do servidor. Tente novamente mais tarde." });
            }
        }

    }
}
