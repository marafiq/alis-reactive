#if NET10_0_OR_GREATER
using System.Threading.Tasks;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The answer an endpoint gives when the server's check of what a request sent fails.
/// </summary>
public static class ErrorResponses
{
    /// <summary>Answers a failed check with status 400 and its messages by the name each input was sent under.</summary>
    /// <remarks>
    /// The answer is MVC's validation problem, as <c>ValidationProblem</c> gives it, whose <c>errors</c> hold each message
    /// by the name its input was sent under, <c>{"errors": {"Room.Number": ["Room is full"]}}</c>, the body both hosts send,
    /// so the page shows each message beside the input it names and every message in the request's summary.
    /// </remarks>
    /// <param name="result">The check's result.</param>
    /// <returns>The action's answer.</returns>
    public static ActionResult ToErrorResponse(this ValidationResult result) => new ErrorResponse(result);
}

internal sealed class ErrorResponse(ValidationResult result) : ActionResult
{
    public override Task ExecuteResultAsync(ActionContext context)
    {
        var failures = new ModelStateDictionary(int.MaxValue);
        foreach (var (field, messages) in result.ErrorsByField)
        {
            foreach (var message in messages)
                failures.AddModelError(field, message);
        }
        var problems = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = problems.CreateValidationProblemDetails(context.HttpContext, failures, StatusCodes.Status400BadRequest);
        return new BadRequestObjectResult(problem).ExecuteResultAsync(context);
    }
}
#endif
