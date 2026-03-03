using ControleGastos.API.Errors;
using ControleGastos.Application.Services;
using ControleGastos.Application.Validators;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Infrastructure.Data;
using ControleGastos.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TotalsService>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PersonValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TransactionValidator>();
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

app.UseMiddleware<ControleGastos.API.Middleware.ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.Run();

public partial class Program;
