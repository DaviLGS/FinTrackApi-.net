using System.Diagnostics;
using FinTrack.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Middlewares;

/// <summary>
/// Converte exceções em respostas ProblemDetails (RFC 7807):
/// ValidationException → 400, NotFoundException → 404, BusinessRuleException → 422, demais → 500.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var problem = CriarProblemDetails(ex);

            if (problem.Status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);

            problem.Instance = context.Request.Path;
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

            context.Response.StatusCode = problem.Status!.Value;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
        }
    }

    private static ProblemDetails CriarProblemDetails(Exception ex) => ex switch
    {
        ValidationException validation => new ValidationProblemDetails(
            validation.Errors
                .GroupBy(e => ParaCamelCase(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos são inválidos."
        },
        NotFoundException => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso não encontrado.",
            Detail = ex.Message
        },
        BusinessRuleException => new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Regra de negócio violada.",
            Detail = ex.Message
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno no servidor."
        }
    };

    private static string ParaCamelCase(string nome) =>
        string.IsNullOrEmpty(nome) ? nome : char.ToLowerInvariant(nome[0]) + nome[1..];
}
