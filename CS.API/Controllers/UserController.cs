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

    public UserController(ProjetoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // Endpoint para obter informações do usuário logado
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile()
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