using CS.API.Data;
using CS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Adicionar pol�ticas CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Adicionar a autentica��o com JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration.GetSection("Jwt:SecretKey").Value;
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("Jwt:SecretKey n�o configurado no appsettings.json.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true, 
            ClockSkew = TimeSpan.Zero
        };
    });

// Registrar o servi�o AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Adicionar os controllers
builder.Services.AddControllers();

// Swagger para a documenta��o da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o do contexto de banco de dados
builder.Services.AddDbContext<ProjetoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CampusSyncConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
