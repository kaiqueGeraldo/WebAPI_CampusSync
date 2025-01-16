using CS.API.Data;
using CS.API.Services;
using CS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ProjetoDbContext _context;
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    public UserController(ProjetoDbContext context, IAuthService authService, ILogger<UserController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    // Endpoint para obter informações do usuário logado
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile()
    {
        var cpf = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(cpf))
        {
            return Unauthorized(new { message = "Usuário não autenticado ou CPF ausente." });
        }

        var user = await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.CPF == cpf);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        var usuarioDTO = new UserDTO
        {
            CPF = user.CPF,
            Nome = user.Nome,
            Email = user.Email,
            UrlImagem = user.UrlImagem,
            UniversidadeNome = user.UniversidadeNome,
            UniversidadeCNPJ = user.UniversidadeCNPJ,
            UniversidadeContatoInfo = user.UniversidadeContatoInfo
        };

        return Ok(usuarioDTO);
    }

    [HttpGet("achievements")]
    [Authorize]
    public async Task<IActionResult> GetUserAchievements()
    {
        var cpf = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(cpf))
        {
            return Unauthorized(new { message = "Usuário não autenticado ou CPF ausente." });
        }

        var user = await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.CPF == cpf);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        // Buscar as faculdades criadas pelo usuário
        var facultiesCreated = await _context.Faculdades
                                             .Where(f => f.UserCPF == user.CPF)
                                             .CountAsync();

        // Buscar os cursos criados nas faculdades do usuário
        var coursesCreated = await _context.Cursos
                                           .Where(c => _context.Faculdades
                                                               .Where(f => f.UserCPF == user.CPF)
                                                               .Select(f => f.Id)
                                                               .Contains(c.FaculdadeId))
                                           .CountAsync();

        // Buscar os estudantes matriculados nas turmas dos cursos das faculdades do usuário
        var enrollmentsCompleted = await _context.Estudantes
            .Where(e => _context.Turmas
                .Where(t => _context.Cursos
                    .Where(c => _context.Faculdades
                        .Where(f => f.UserCPF == user.CPF)
                        .Select(f => f.Id)
                        .Contains(c.FaculdadeId))
                    .Select(c => c.Id)
                    .Contains(t.CursoId))
                .Select(t => t.Id)
                .Contains(e.TurmaId))
            .CountAsync();

        var achievements = new
        {
            FacultiesCreated = facultiesCreated,
            CoursesCreated = coursesCreated,
            EnrollmentsCompleted = enrollmentsCompleted
        };

        return Ok(achievements);
    }

    // Endpoint para verificar se um CPF está cadastrado
    [HttpGet("verify-cpf")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCpf([FromQuery] string cpf)
    {
        if (!IsValidCpf(cpf))
        {
            return BadRequest("CPF inválido.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CPF == cpf);

        if (user == null)
        {
            return NotFound("CPF não encontrado.");
        }

        return Ok(new { Message = "CPF está cadastrado." });
    }

    // Endpoint para verificar se um e-mail está cadastrado
    [HttpGet("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email)
    {
        if (!IsValidEmail(email))
        {
            return BadRequest("E-mail inválido.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return NotFound("E-mail não encontrado.");
        }

        return Ok(new { Message = "E-mail está cadastrado." });
    }

    // Endpoint para atualizar as informações do usuário logado
    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UserUpdateRequest request)
    {
        var cpf = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(cpf))
        {
            return Unauthorized("Usuário não autenticado.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CPF == cpf);

        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        // Atualizações condicionais
        user.Nome = !string.IsNullOrEmpty(request.Nome) ? request.Nome : user.Nome;
        user.Email = !string.IsNullOrEmpty(request.Email) && IsValidEmail(request.Email) ? request.Email : user.Email;
        user.UrlImagem = !string.IsNullOrEmpty(request.UrlImagem) ? request.UrlImagem : user.UrlImagem;

        // Dados da universidade
        user.UniversidadeNome = !string.IsNullOrEmpty(request.UniversidadeNome) ? request.UniversidadeNome : user.UniversidadeNome;
        user.UniversidadeCNPJ = !string.IsNullOrEmpty(request.UniversidadeCNPJ) ? request.UniversidadeCNPJ : user.UniversidadeCNPJ;
        user.UniversidadeContatoInfo = !string.IsNullOrEmpty(request.UniversidadeContatoInfo) ? request.UniversidadeContatoInfo : user.UniversidadeContatoInfo;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var usuarioDTO = new UserDTO
        {
            Nome = user.Nome,
            Email = user.Email,
            UrlImagem = user.UrlImagem,
            UniversidadeNome = user.UniversidadeNome,
            UniversidadeCNPJ = user.UniversidadeCNPJ,
            UniversidadeContatoInfo = user.UniversidadeContatoInfo,
            Token = _authService.CreateToken(user)
        };

        return Ok(usuarioDTO);
    }

    // Endpoint para listar usuários com paginação
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
    {
        var users = await _context.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.CPF,
                u.Nome,
                u.Email,
                u.UniversidadeNome,
                u.Faculdades
            })
            .ToListAsync();

        return Ok(users);
    }

    // DELETE: api/User
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
        try
        {
            var cpf = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(cpf))
            {
                return BadRequest(new { message = "CPF não encontrado nos dados de autenticação." });
            }

            var user = await _context.Users
                .Include(u => u.Faculdades)
                    .ThenInclude(f => f.Cursos)
                        .ThenInclude(c => c.Turmas)
                            .ThenInclude(t => t.Estudantes)
                .Include(u => u.Faculdades)
                    .ThenInclude(f => f.Cursos)
                        .ThenInclude(c => c.Disciplinas)
                .Include(u => u.Faculdades)
                    .ThenInclude(f => f.Cursos)
                        .ThenInclude(c => c.Colaborador)
                .Include(u => u.Colaboradores)
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            // Exclusão manual das dependências
            if (user.Faculdades != null)
            {
                foreach (var faculdade in user.Faculdades)
                {
                    if (faculdade.Cursos != null)
                    {
                        foreach (var curso in faculdade.Cursos)
                        {
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

                            _context.Cursos.Remove(curso);
                        }
                    }

                    _context.Faculdades.Remove(faculdade);
                }
            }

            if (user.Colaboradores != null && user.Colaboradores.Any())
            {
                _context.Colaboradores.RemoveRange(user.Colaboradores);
            }

            // Removendo o usuário
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuário deletado com sucesso." });
        }
        catch (Exception ex)
        {
            // Logando o erro
            _logger.LogError($"Erro ao excluir conta: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "Erro interno do servidor. Tente novamente mais tarde." });
        }
    }

    // Função auxiliar para validar cpf
    private bool IsValidCpf(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            return false;

        int CalculateDigit(List<int> digits, int factor)
        {
            int sum = 0;
            for (int i = 0; i < digits.Count; i++)
            {
                sum += digits[i] * factor--;
            }

            int remainder = (sum * 10) % 11;
            return remainder == 10 ? 0 : remainder;
        }

        var digits = cpf.Select(c => int.Parse(c.ToString())).ToList();

        int digit1 = CalculateDigit(digits.Take(9).ToList(), 10);
        int digit2 = CalculateDigit(digits.Take(10).ToList(), 11);

        return digit1 == digits[9] && digit2 == digits[10];
    }

    // Função auxiliar para validar e-mails
    private bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
    }
}