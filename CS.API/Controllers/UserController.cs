using CS.API.Data;
using CS.API.Services;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> GetUserProfile([FromQuery] string cpf)
    {
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

    // Endpoint para verificar se um e-mail está cadastrado
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return NotFound("E-mail não encontrado.");
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

    // Endpoint para atualizar as informações do usuário e da universidade
    [HttpPut("update")]
    public async Task<IActionResult> Update(UserUpdateRequest request)
    {
        if (string.IsNullOrEmpty(request.CPF))
        {
            return BadRequest("CPF é obrigatório.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CPF == request.CPF);

        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        user.Nome = request.Nome ?? user.Nome;
        user.Email = request.Email ?? user.Email;
        user.UrlImagem = request.UrlImagem ?? user.UrlImagem;

        user.UniversidadeNome = request.UniversidadeNome ?? user.UniversidadeNome;
        user.UniversidadeCNPJ = request.UniversidadeCNPJ ?? user.UniversidadeCNPJ;
        user.UniversidadeContatoInfo = request.UniversidadeContatoInfo ?? user.UniversidadeContatoInfo;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var usuarioDTO = new UserDTO
        {
            CPF = user.CPF,
            Nome = user.Nome,
            Email = user.Email,
            UrlImagem = user.UrlImagem,
            Token = _authService.CreateToken(user),
            UniversidadeNome = user.UniversidadeNome,
            UniversidadeCNPJ = user.UniversidadeCNPJ,
            UniversidadeContatoInfo = user.UniversidadeContatoInfo
        };

        return Ok(usuarioDTO);
    }

    // Exemplo de endpoint para listar todos os usuários (se necessário)
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }
}