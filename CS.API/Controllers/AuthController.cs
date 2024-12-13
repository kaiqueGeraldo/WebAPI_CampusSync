using CS.API.Data;
using CS.API.Services;
using CS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ProjetoDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(IConfiguration configuration, ProjetoDbContext context, IAuthService authService)
    {
        _configuration = configuration;
        _context = context;
        _authService = authService;
    }


    #region Password Handling

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    #endregion

    #region Authentication Endpoints

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLogin login)
    {
        if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
        {
            return BadRequest("Email e senha são obrigatórios.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);

        if (user == null || !VerifyPasswordHash(login.Password, user.PasswordHash, user.PasswordSalt))
        {
            await Task.Delay(500);
            return Unauthorized("Credenciais inválidas.");
        }

        var token = _authService.CreateToken(user);

        var userDTO = MapUserToDTO(user, token);

        return Ok(userDTO);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegister request)
    {
        if (string.IsNullOrWhiteSpace(request.CPF) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("CPF, Email e Senha são obrigatórios.");
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.CPF == request.CPF || u.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest("Já existe um usuário com este CPF ou Email.");
        }

        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            CPF = request.CPF,
            Nome = request.Nome,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            UrlImagem = request.UrlImagem,
            UniversidadeNome = request.UniversidadeNome,
            UniversidadeCNPJ =  request.UniversidadeCNPJ,
            UniversidadeContatoInfo = request.UniversidadeContatoInfo
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.CreateToken(user);

        var userDTO = new UserDTO
        {
            CPF = user.CPF,
            Nome = user.Nome,
            Email = user.Email,
            Token = token,
            UrlImagem = user.UrlImagem,
            UniversidadeNome = user.UniversidadeNome,
            UniversidadeCNPJ = user.UniversidadeCNPJ,
            UniversidadeContatoInfo = user.UniversidadeContatoInfo
        };

        return Ok(userDTO);
    }

    #endregion

    #region Password Management

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(UserChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CPF) || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest("Todos os campos são obrigatórios.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CPF == request.CPF);

        if (user == null || !VerifyPasswordHash(request.OldPassword, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest("Credenciais inválidas.");
        }

        CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok("Senha alterada com sucesso.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(UserPasswordResetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CPF) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest("Todos os campos são obrigatórios.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.CPF == request.CPF);

        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok("Senha redefinida com sucesso.");
    }

    #endregion

    #region Helper Methods

    private UserDTO MapUserToDTO(User user, string token)
    {
        return new UserDTO
        {
            CPF = user.CPF,
            Nome = user.Nome,
            Email = user.Email,
            UrlImagem = user.UrlImagem,
            Token = token,
            UniversidadeNome = user.UniversidadeNome,
            UniversidadeCNPJ = user.UniversidadeCNPJ,
            UniversidadeContatoInfo = user.UniversidadeContatoInfo
        };
    }

    #endregion
}