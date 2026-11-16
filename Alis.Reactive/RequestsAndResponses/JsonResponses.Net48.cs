#if NET48
using System.Text;
using System.Text.Json;
using System.Web.Mvc;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The answer an MVC 5 action gives with a JSON body, written as the app's JSON options write it.
/// </summary>
public static class JsonResponses
{
    /// <summary>Answers with the body as JSON, written under the options the app registers with <c>AddReactiveJson</c>.</summary>
    /// <remarks>
    /// MVC 5's own <c>Json</c> writes each member by its C# name and a date as <c>\/Date(ms)\/</c> after moving it to
    /// UTC, so a date of birth can show the day before. This answer writes each date in ISO 8601 and each member by the
    /// name the app's options give it, the body a page reads on both hosts: <c>return this.JsonResponse(visit);</c>.
    /// An app that never registered its options, or set no MVC resolver, is refused when the action answers.
    /// </remarks>
    /// <typeparam name="TBody">The class of the body, whose members are written.</typeparam>
    /// <param name="controller">The action's controller.</param>
    /// <param name="body">The body.</param>
    /// <returns>The action's answer.</returns>
    public static ActionResult JsonResponse<TBody>(this Controller controller, TBody body) => new JsonBodyResponse<TBody>(body);
}

internal sealed class JsonBodyResponse<TBody>(TBody body) : ActionResult
{
    public override void ExecuteResult(ControllerContext context)
    {
        var request = context.HttpContext.Request;
        var options = RegisteredJson.ReadBy(request.HttpMethod + " " + request.Path);
        var response = context.HttpContext.Response;
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF8;
        JsonSerializer.Serialize(response.OutputStream, body, options);
    }
}
#endif
