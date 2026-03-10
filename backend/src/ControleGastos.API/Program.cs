using ControleGastos.API.Errors;
using ControleGastos.API.Auth;
using ControleGastos.API.Middleware;
using ControleGastos.Application.Interfaces;
using ControleGastos.Application.Services;
using ControleGastos.Application.Validators;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Infrastructure.Data;
using ControleGastos.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var baseConnectionString = !string.IsNullOrWhiteSpace(envConnectionString)
    ? envConnectionString
    : configuredConnectionString
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var dbPassword = builder.Configuration["Database:Password"]
    ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString);

if (!string.IsNullOrWhiteSpace(dbPassword))
{
    connectionStringBuilder.Password = dbPassword;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionStringBuilder.ConnectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Auth:Jwt"));
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("Auth:Google"));
builder.Services.Configure<RefreshCookieOptions>(builder.Configuration.GetSection("Auth:RefreshCookie"));

var jwtOptions = builder.Configuration.GetSection("Auth:Jwt").Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Auth:Jwt configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
    throw new InvalidOperationException("Auth:Jwt:Secret is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ErrorEnvelope
                {
                    Type = "unauthorized",
                    Message = "Autenticação obrigatória.",
                    Errors = new Dictionary<string, string[]>()
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ErrorEnvelope
                {
                    Type = "forbidden",
                    Message = "Acesso negado.",
                    Errors = new Dictionary<string, string[]>()
                });
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TotalsService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenFactory, Sha256RefreshTokenFactory>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IGoogleTokenValidator, FakeGoogleTokenValidator>();
}
else
{
    builder.Services.AddHttpClient<IGoogleTokenValidator, GoogleTokenInfoValidator>();
}

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PersonValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TransactionValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AuthRegisterValidator>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor invalido." : e.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var response = new ErrorEnvelope
        {
            Type = "validation_error",
            Message = "Dados invalidos.",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
