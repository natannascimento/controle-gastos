using ControleGastos.API.Errors;
using ControleGastos.Application.Exceptions;
using FluentValidation;

namespace ControleGastos.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

            await WriteEnvelopeAsync(
                context,
                StatusCodes.Status400BadRequest,
                "validation_error",
                "Dados invalidos.",
                errors);
        }
        catch (NotFoundException ex)
        {
            await WriteEnvelopeAsync(
                context,
                StatusCodes.Status404NotFound,
                "not_found",
                ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteEnvelopeAsync(
                context,
                StatusCodes.Status400BadRequest,
                "business_rule",
                ex.Message);
        }
        catch (Exception)
        {
            await WriteEnvelopeAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "unexpected_error",
                "Erro inesperado.");
        }
    }

    private static async Task WriteEnvelopeAsync(
        HttpContext context,
        int statusCode,
        string type,
        string message,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = statusCode;

        var envelope = new ErrorEnvelope
        {
            Type = type,
            Message = message,
            Errors = errors ?? new Dictionary<string, string[]>()
        };

        await context.Response.WriteAsJsonAsync(envelope);
    }
}
