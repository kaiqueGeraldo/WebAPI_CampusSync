using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColaboradorController : ControllerBase
    {
        private readonly ProjetoDbContext _context;

        public ColaboradorController(ProjetoDbContext context)
        {
            _context = context;
        }

        // GET: api/Colaborador
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ColaboradorResponse>>> GetColaboradores()
        {
            var colaboradores = await _context.Colaboradores
                .Include(c => c.Curso)
                .Select(c => new ColaboradorResponse
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Cargo = c.Cargo,
                    NumeroRegistro = c.NumeroRegistro,
                    DataAdmissao = c.DataAdmissao,
                    CursoNome = c.Curso != null ? c.Curso.Nome : null
                })
                .ToListAsync();

            return Ok(colaboradores);
        }

        // GET: api/Colaborador/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ColaboradorResponse>> GetColaborador(int id)
        {
            var colaborador = await _context.Colaboradores
                .Include(c => c.Curso)
                .Where(c => c.Id == id)
                .Select(c => new ColaboradorResponse
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Cargo = c.Cargo,
                    NumeroRegistro = c.NumeroRegistro,
                    DataAdmissao = c.DataAdmissao,
                    CursoNome = c.Curso != null ? c.Curso.Nome : null
                })
                .FirstOrDefaultAsync();

            if (colaborador == null)
            {
                return NotFound();
            }

            return Ok(colaborador);
        }

        // POST: api/Colaborador
        [HttpPost]
        public async Task<ActionResult<ColaboradorResponse>> CreateColaborador(ColaboradorRequest colaboradorRequest)
        {
            var curso = await _context.Cursos.FindAsync(colaboradorRequest.CursoId);
            if (curso == null)
            {
                return BadRequest("Curso associado não encontrado.");
            }

            var colaborador = new Colaborador
            {
                Nome = colaboradorRequest.Nome,
                CPF = colaboradorRequest.CPF,
                RG = colaboradorRequest.RG,
                Telefone = colaboradorRequest.Telefone,
                TituloEleitor = colaboradorRequest.TituloEleitor,
                EstadoCivil = colaboradorRequest.EstadoCivil,
                Nacionalidade = colaboradorRequest.Nacionalidade,
                CorRacaEtnia = colaboradorRequest.CorRacaEtnia,
                Escolaridade = colaboradorRequest.Escolaridade,
                NomePai = colaboradorRequest.NomePai,
                NomeMae = colaboradorRequest.NomeMae,
                DataNascimento = colaboradorRequest.DataNascimento,
                Endereco = new Endereco
                {
                    Logradouro = colaboradorRequest.Endereco.Logradouro,
                    Numero = colaboradorRequest.Endereco.Numero,
                    Bairro = colaboradorRequest.Endereco.Bairro,
                    Cidade = colaboradorRequest.Endereco.Cidade,
                    Estado = colaboradorRequest.Endereco.Estado,
                    CEP = colaboradorRequest.Endereco.CEP
                },
                Cargo = colaboradorRequest.Cargo,
                NumeroRegistro = colaboradorRequest.NumeroRegistro,
                DataAdmissao = colaboradorRequest.DataAdmissao,
                Curso = curso
            };

            _context.Colaboradores.Add(colaborador);
            await _context.SaveChangesAsync();

            var colaboradorResponse = new ColaboradorResponse
            {
                Id = colaborador.Id,
                Nome = colaborador.Nome,
                Cargo = colaborador.Cargo,
                NumeroRegistro = colaborador.NumeroRegistro,
                DataAdmissao = colaborador.DataAdmissao,
                CursoNome = colaborador.Curso?.Nome
            };

            return CreatedAtAction("GetColaborador", new { id = colaborador.Id }, colaboradorResponse);
        }

        // PUT: api/Colaborador/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateColaborador(int id, ColaboradorRequest colaboradorRequest)
        {
            if (id != colaboradorRequest.Id)
            {
                return BadRequest();
            }

            var colaborador = await _context.Colaboradores
                .Include(c => c.Curso)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colaborador == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(colaboradorRequest.CursoId);
            if (curso == null)
            {
                return BadRequest("Curso associado não encontrado.");
            }

            colaborador.Nome = colaboradorRequest.Nome;
            colaborador.CPF = colaboradorRequest.CPF;
            colaborador.RG = colaboradorRequest.RG;
            colaborador.Telefone = colaboradorRequest.Telefone;
            colaborador.TituloEleitor = colaboradorRequest.TituloEleitor;
            colaborador.EstadoCivil = colaboradorRequest.EstadoCivil;
            colaborador.Nacionalidade = colaboradorRequest.Nacionalidade;
            colaborador.CorRacaEtnia = colaboradorRequest.CorRacaEtnia;
            colaborador.Escolaridade = colaboradorRequest.Escolaridade;
            colaborador.NomePai = colaboradorRequest.NomePai;
            colaborador.NomeMae = colaboradorRequest.NomeMae;
            colaborador.DataNascimento = colaboradorRequest.DataNascimento;
            colaborador.Cargo = colaboradorRequest.Cargo;
            colaborador.NumeroRegistro = colaboradorRequest.NumeroRegistro;
            colaborador.DataAdmissao = colaboradorRequest.DataAdmissao;
            colaborador.Curso = curso;

            _context.Entry(colaborador).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Colaborador/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColaborador(int id)
        {
            var colaborador = await _context.Colaboradores.FindAsync(id);
            if (colaborador == null)
            {
                return NotFound();
            }

            _context.Colaboradores.Remove(colaborador);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
