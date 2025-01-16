using Azure.Core;
using CS.API.Data;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColaboradorController : ControllerBase
    {
        private readonly ProjetoDbContext _context;
        private readonly ILogger<ColaboradorController> _logger;

        public ColaboradorController(ProjetoDbContext context, ILogger<ColaboradorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Colaborador?cpf=12345678900
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ColaboradorResponse>>> GetColaboradores([FromQuery] string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                return BadRequest("O CPF deve ser informado.");
            }

            // Obtenha o usuário associado ao CPF
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Obtenha os colaboradores relacionados ao usuário
            var colaboradores = await _context.Colaboradores
                .Include(c => c.Pessoa)
                .Include(c => c.Curso)
                .Where(c => c.UserCPFColaboradores == cpf)
                .Select(c => new ColaboradorResponse
                {
                    Id = c.Id,
                    Nome = c.Pessoa.Nome,
                    CPF = c.Pessoa.CPF,
                    RG = c.Pessoa.RG,
                    Email = c.Pessoa.Email,
                    Telefone = c.Pessoa.Telefone,
                    Cargo = c.Cargo,
                    NumeroRegistro = c.NumeroRegistro,
                    DataAdmissao = c.DataAdmissao,
                    DataNascimento = c.Pessoa.DataNascimento,
                    NomePai = c.Pessoa.NomePai,
                    NomeMae = c.Pessoa.NomeMae,
                    Endereco = c.Pessoa.Endereco,
                    CursoNome = c.Cargo == "Docente" && c.Curso != null ? c.Curso.Nome : null,
                    UniversidadeNome = c.User.UniversidadeNome
                })
                .ToListAsync();

            if (!colaboradores.Any())
            {
                return NotFound("Nenhum colaborador encontrado para o CPF informado.");
            }

            return Ok(colaboradores);
        }

        // GET: api/Colaborador/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ColaboradorResponse>> GetColaborador(int id)
        {
            var colaborador = await _context.Colaboradores
                .Include(c => c.Curso)
                .Include(c => c.Pessoa)
                .Where(c => c.Id == id)
                .Select(c => new ColaboradorResponse
                {
                    Id = c.Id,
                    Nome = c.Pessoa.Nome,
                    CPF = c.Pessoa.CPF,
                    RG = c.Pessoa.RG,
                    Email = c.Pessoa.Email,
                    Telefone = c.Pessoa.Telefone,
                    Cargo = c.Cargo,
                    NumeroRegistro = c.NumeroRegistro,
                    DataAdmissao = c.DataAdmissao,
                    DataNascimento = c.Pessoa.DataNascimento,
                    NomePai = c.Pessoa.NomePai,
                    NomeMae = c.Pessoa.NomeMae,
                    Endereco = c.Pessoa.Endereco,
                    CursoNome = c.Curso.Nome,
                    UniversidadeNome = c.User.UniversidadeNome
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.CPF == colaboradorRequest.UserCPFColaboradores);

            if (colaboradorRequest.Cargo == "Docente" && colaboradorRequest.CursoId == null)
            {
                return BadRequest("O campo Curso é obrigatório para o cargo 'Docente'.");
            }
            else if (user == null)
            {
                return BadRequest("O CPF informado não corresponde à um usuário.");
            }

            Curso? curso = null;
            if (colaboradorRequest.CursoId != null)
            {
                curso = await _context.Cursos.FindAsync(colaboradorRequest.CursoId);
                if (curso == null)
                {
                    return BadRequest("Curso associado não encontrado.");
                }
            }

            var pessoa = new Pessoa
            {
                Nome = colaboradorRequest.Nome,
                CPF = colaboradorRequest.CPF,
                RG = colaboradorRequest.RG,
                Email = colaboradorRequest.Email,
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
                }
            };

            _context.Pessoas.Add(pessoa);
            await _context.SaveChangesAsync();

            var colaborador = new Colaborador
            {
                PessoaId = pessoa.Id,
                Cargo = colaboradorRequest.Cargo,
                NumeroRegistro = colaboradorRequest.NumeroRegistro,
                DataAdmissao = colaboradorRequest.DataAdmissao,
                Curso = curso,
                UserCPFColaboradores = colaboradorRequest.UserCPFColaboradores,
            };

            _context.Colaboradores.Add(colaborador);
            await _context.SaveChangesAsync();

            var colaboradorResponse = new ColaboradorResponse
            {
                Nome = pessoa.Nome,
                CPF = pessoa.CPF,
                RG = pessoa.RG,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone,
                Cargo = colaborador.Cargo,
                NumeroRegistro = colaborador.NumeroRegistro,
                DataAdmissao = colaborador.DataAdmissao,
                DataNascimento = pessoa.DataNascimento,
                Endereco = pessoa.Endereco,
                CursoNome = colaborador.Curso?.Nome,
                UniversidadeNome = colaborador.User?.UniversidadeNome
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
                .Include(c => c.Pessoa)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colaborador == null)
            {
                return NotFound();
            }

            // Atualizar apenas os campos enviados no request
            if (!string.IsNullOrEmpty(colaboradorRequest.Nome))
            {
                colaborador.Pessoa.Nome = colaboradorRequest.Nome;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.CPF))
            {
                colaborador.Pessoa.CPF = colaboradorRequest.CPF;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.RG))
            {
                colaborador.Pessoa.RG = colaboradorRequest.RG;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.Telefone))
            {
                colaborador.Pessoa.Telefone = colaboradorRequest.Telefone;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.TituloEleitor))
            {
                colaborador.Pessoa.TituloEleitor = colaboradorRequest.TituloEleitor;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.EstadoCivil))
            {
                colaborador.Pessoa.EstadoCivil = colaboradorRequest.EstadoCivil;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.Nacionalidade))
            {
                colaborador.Pessoa.Nacionalidade = colaboradorRequest.Nacionalidade;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.CorRacaEtnia))
            {
                colaborador.Pessoa.CorRacaEtnia = colaboradorRequest.CorRacaEtnia;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.Escolaridade))
            {
                colaborador.Pessoa.Escolaridade = colaboradorRequest.Escolaridade;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.NomePai))
            {
                colaborador.Pessoa.NomePai = colaboradorRequest.NomePai;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.NomeMae))
            {
                colaborador.Pessoa.NomeMae = colaboradorRequest.NomeMae;
            }

            if (colaboradorRequest.DataNascimento != null)
            {
                colaborador.Pessoa.DataNascimento = colaboradorRequest.DataNascimento;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.Cargo))
            {
                colaborador.Cargo = colaboradorRequest.Cargo;
            }

            if (!string.IsNullOrEmpty(colaboradorRequest.NumeroRegistro))
            {
                colaborador.NumeroRegistro = colaboradorRequest.NumeroRegistro;
            }

            if (colaboradorRequest.DataAdmissao != null)
            {
                colaborador.DataAdmissao = colaboradorRequest.DataAdmissao;
            }

            if (colaboradorRequest.Endereco != null)
            {
                colaborador.Pessoa.Endereco = colaboradorRequest.Endereco;
            }

            if (colaboradorRequest.UserCPFColaboradores != null)
            {
                colaborador.UserCPFColaboradores = colaboradorRequest.UserCPFColaboradores;
            }

            // Verificar e atualizar Curso somente se enviado no request
            if (colaboradorRequest.CursoId.HasValue)
            {
                var curso = await _context.Cursos.FindAsync(colaboradorRequest.CursoId.Value);
                if (curso == null)
                {
                    return BadRequest("Curso associado não encontrado.");
                }
                colaborador.Curso = curso;
            }

            _context.Entry(colaborador).State = EntityState.Modified;
            _context.Entry(colaborador.Pessoa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Colaborador/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColaborador(int id)
        {
            try
            {
                var colaborador = await _context.Colaboradores
                    .Include(c => c.Pessoa)
                    .Include(c => c.Curso)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (colaborador == null)
                {
                    return NotFound(new { message = "Colaborador não encontrado." });
                }

                // Remove o colaborador e suas relações associadas em cascata
                _context.Colaboradores.Remove(colaborador);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir colaborador: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno do servidor. Tente novamente mais tarde." });
            }
        }
    }
}
