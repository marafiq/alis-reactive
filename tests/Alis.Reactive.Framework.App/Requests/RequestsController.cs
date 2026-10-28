using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// The endpoints a page's requests reach, each binding its request as any endpoint does and saying, in its
/// own words, what it received (capabilities of the HTTP pipeline, and M4 in a request).
/// </summary>
public sealed class RequestsController : Controller
{
    public IActionResult Desk() => View(new RequestsPage());

    [HttpGet("/Requests/Residents/{id}")]
    public IActionResult Resident(int id, [FromQuery] string? wing) => Said($"resident {id} in {wing ?? "no wing"}");

    [HttpPost("/Requests/Notes")]
    public IActionResult Note([FromBody] NoteRequest note) =>
        Said($"note '{note.Text}' titled {note.Title ?? "nothing"}, tally {note.Tally}, amount {Number(note.Amount)}");

    [HttpPut("/Requests/Notes/{id}")]
    public IActionResult PutNote(int id, [FromBody] NoteRequest note) => Said($"put note {id}: '{note.Text}'");

    [HttpDelete("/Requests/Notes/{id}")]
    public IActionResult DeleteNote(int id) => Said($"deleted note {id}");

    [HttpPost("/Requests/Upload")]
    public IActionResult Upload([FromForm] NoteRequest note) =>
        Said($"form '{note.Text}' from {(Request.HasFormContentType ? "form data" : "something else")}, amount {Number(note.Amount)}");

    [HttpGet("/Requests/Headers")]
    public IActionResult Headers() => Said($"shift {Request.Headers["X-Shift"]} at the {Request.Headers["X-Desk"]} desk");

    [HttpGet("/Requests/Totals/{total}")]
    public IActionResult Total(decimal total) => Said($"total {Number(total)} in the path");

    /// <summary>Refuses with the status asked for: 409 says why, in a body of its own.</summary>
    [HttpGet("/Requests/Refuse")]
    public IActionResult Refuse([FromQuery] int status) =>
        status == 409 ? StatusCode(409, new Refusal { Reason = "The room is taken" }) : StatusCode(status);

    [HttpGet("/Requests/First")]
    public IActionResult First() => Json(new FirstAnswer { Next = "the second step" });

    [HttpGet("/Requests/Second")]
    public IActionResult Second([FromQuery] string from) => Said($"second, after {from}");

    /// <summary>Answers after the delay asked for, with the name it was given.</summary>
    [HttpGet("/Requests/Slow")]
    public async Task<IActionResult> Slow([FromQuery] int delay, [FromQuery] string name)
    {
        await Task.Delay(delay);
        return Said($"{name} answered");
    }

    private JsonResult Said(string summary) => Json(new Echo { Summary = summary });

    private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
