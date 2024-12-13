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

        public EstudanteController(ProjetoDbContext context)
        {
            _context = context;
        }

        // GET: api/Estudante
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstudanteResponse>>> GetEstudantes()
        {
            var estudantes = await _context.Estudantes
                .Include(e => e.Turma)
               .Select(e => new EstudanteResponse
               {
                   Nome = e.Nome,
                   CPF = e.CPF,
                   RG = e.RG,
                   Email = e.Email,
                   Telefone = e.Telefone,
                   NumeroMatricula = e.NumeroMatricula,
                   DataMatricula = e.DataMatricula,
                   DataNascimento = e.DataNascimento,
                   NomePai = e.NomePai,
                   NomeMae = e.NomeMae,
                   TelefonePai = e.TelefonePai,
                   TelefoneMae = e.TelefoneMae,
                   Endereco = e.Endereco,
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
                .Where(e => e.Id == id)
                .Select(e => new EstudanteResponse
                {
                    Nome = e.Nome,
                    CPF = e.CPF,
                    RG = e.RG,
                    Email = e.Email,
                    Telefone = e.Telefone,
                    NumeroMatricula = e.NumeroMatricula,
                    DataMatricula = e.DataMatricula,
                    DataNascimento = e.DataNascimento,
                    NomePai = e.NomePai,
                    NomeMae = e.NomeMae,
                    TelefonePai = e.TelefonePai,
                    TelefoneMae = e.TelefoneMae,
                    Endereco = e.Endereco,
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
            var estudante = new Estudante
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
                NumeroMatricula = estudanteRequest.NumeroMatricula,
                DataMatricula = estudanteRequest.DataMatricula,
                TelefonePai = estudanteRequest.TelefonePai,
                TelefoneMae = estudanteRequest.TelefoneMae,
                TurmaId = estudanteRequest.TurmaId
            };

            _context.Estudantes.Add(estudante);
            await _context.SaveChangesAsync();

            var estudanteResponse = new EstudanteResponse
            {
                Nome = estudante.Nome,
                CPF = estudante.CPF,
                RG = estudante.RG,
                Email = estudante.Email,
                Telefone = estudante.Telefone,
                NumeroMatricula = estudante.NumeroMatricula,
                DataMatricula = estudante.DataMatricula,
                DataNascimento = estudante.DataNascimento,
                NomePai = estudante.NomePai,
                NomeMae = estudante.NomeMae,
                TelefonePai = estudante.TelefonePai,
                TelefoneMae = estudante.TelefoneMae,
                Endereco = estudante.Endereco,
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

            var estudante = await _context.Estudantes.FindAsync(id);
            if (estudante == null)
            {
                return NotFound();
            }

            estudante.Nome = estudanteRequest.Nome;
            estudante.CPF = estudanteRequest.CPF;
            estudante.RG = estudanteRequest.RG;
            estudante.Telefone = estudanteRequest.Telefone;
            estudante.TituloEleitor = estudanteRequest.TituloEleitor;
            estudante.EstadoCivil = estudanteRequest.EstadoCivil;
            estudante.Nacionalidade = estudanteRequest.Nacionalidade;
            estudante.CorRacaEtnia = estudanteRequest.CorRacaEtnia;
            estudante.Escolaridade = estudanteRequest.Escolaridade;
            estudante.NomePai = estudanteRequest.NomePai;
            estudante.NomeMae = estudanteRequest.NomeMae;
            estudante.DataNascimento = estudanteRequest.DataNascimento;
            estudante.NumeroMatricula = estudanteRequest.NumeroMatricula;
            estudante.DataMatricula = estudanteRequest.DataMatricula;
            estudante.TelefonePai = estudanteRequest.TelefonePai;
            estudante.TelefoneMae = estudanteRequest.TelefoneMae;
            estudante.Endereco = estudanteRequest.Endereco;
            estudante.TurmaId = estudanteRequest.TurmaId;

            _context.Entry(estudante).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Estudante/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstudante(int id)
        {
            var estudante = await _context.Estudantes.FindAsync(id);
            if (estudante == null)
            {
                return NotFound();
            }

            _context.Estudantes.Remove(estudante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
