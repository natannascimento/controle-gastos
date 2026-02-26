using ControleGastos.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Erro inesperado.");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = message
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
