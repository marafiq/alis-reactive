using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// The endpoints a page's requests reach, each binding its request as any endpoint does and saying, in its
/// own words, what it received (capabilities of the HTTP pipeline, and M4 in a request): members set, inputs
/// named, a file a person chose, and the pages a gather is refused for.
/// </summary>
public sealed class RequestsController : Controller
{
    public IActionResult Desk() => View(new RequestsPage());

    public IActionResult Booking() => View(new BookingPage());

    /// <summary>Refuses the booking as a conflict, with its reason in the body.</summary>
    [HttpPost("/Requests/Bookings/Taken")]
    public IActionResult BookTaken() => Conflict(new BookingProblem { Reason = "room 2 is taken" });

    /// <summary>Refuses the booking as forbidden, with its reason in the body.</summary>
    [HttpPost("/Requests/Bookings/Closed")]
    public IActionResult BookClosed() => StatusCode(403, new BookingProblem { Reason = "the east wing is closed" });

    public IActionResult RouteRefusals(RouteMistake mistake) => View(new RouteRefusalsPage { Mistake = mistake });

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

    [HttpPost("/Requests/Estimates")]
    public IActionResult Estimate([FromBody] EstimateRequest estimate) =>
        Said(estimate.Estimate is decimal value ? $"estimate {Number(value)}" : "no estimate");

    public IActionResult Outage() => View(new OutagePage());

    [HttpPost("/Requests/Ledger")]
    public IActionResult Ledger([FromBody] LedgerNote note) => Said($"ledger titled {note.Title ?? "nothing"}");

    [HttpGet("/Requests/Limits/{limit}")]
    public IActionResult Limit(int limit) => Said($"limit {limit}");

    /// <summary>Drops the connection before it answers, as a server that fails mid-request does.</summary>
    [HttpGet("/Requests/Dropped")]
    public IActionResult Dropped()
    {
        HttpContext.Abort();
        return new EmptyResult();
    }

    /// <summary>Says it answers JSON, then sends JSON that stops part way.</summary>
    [HttpGet("/Requests/Garbled")]
    public IActionResult Garbled() => Content("{\"summary\": \"the night", "application/json");

    [HttpGet("/Requests/Xhtml")]
    public IActionResult Xhtml() => Content("The night desk is open.", "application/xhtml+xml");

    [HttpGet("/Requests/Bytes")]
    public IActionResult Bytes() => File(System.Text.Encoding.UTF8.GetBytes("ledger.bin"), "application/octet-stream");

    public IActionResult Census() => View(new CensusPage());

    /// <summary>Says the query it was sent, as the address reads, and the wing and wings it bound from it.</summary>
    [HttpGet("/Requests/Census/Asked")]
    public IActionResult CensusAsked([FromQuery] CensusQuery query) =>
        Said($"query {QueryText()}, bound wing {query.Wing ?? "none"} and wings {Listed(query.Wings)}");

    /// <summary>Says each field the form post carried, by name and value.</summary>
    [HttpPost("/Requests/Census/Posted")]
    public IActionResult CensusPosted() =>
        Said("fields " + string.Join(", ", Request.Form.Select(field => $"{field.Key}='{field.Value}'")));

    [HttpPost("/Requests/Census/Labels")]
    public IActionResult LabelsKept([FromBody] CensusLabels labels) =>
        Said("labels " + string.Join(" and ", (labels.Labels ?? []).Select(label => label is null ? "null" : $"'{label}'")));

    [HttpGet("/Requests/Census/Cards")]
    public IActionResult CardsAsked() => Said($"asked with query {QueryText()}");

    private string QueryText() => Request.QueryString.HasValue ? Request.QueryString.Value! : "nothing";

    private static string Listed(List<string>? items) =>
        items is null || items.Count == 0 ? "none" : string.Join(" and ", items.Select(item => item ?? "an empty one"));

    public IActionResult Round() => View(new RoundPage { Doses = { [1042] = new() } });

    [HttpPost("/Requests/Rounds")]
    public IActionResult RecordRound([FromForm] RoundPage round) =>
        Said(round.Doses.TryGetValue(1042, out var dose) ? $"dose for 1042: {dose.Amount ?? "none"}" : "no dose for 1042");

    public IActionResult SignIn() => View(new SignInPage());

    [HttpPost("/Requests/SignIns")]
    public IActionResult SignIns([FromBody] SignInRequest signIn) =>
        Said($"{signIn.Visitor ?? "no one"}, party of {signIn.Party}, note {signIn.Note ?? "none"}");

    [HttpPost("/Requests/Passes")]
    public IActionResult Pass([FromBody] PassRequest pass) => Said($"pass {pass.PassID}");

    [HttpPost("/Requests/Parties")]
    public IActionResult Party([FromBody] PartyRequest party) =>
        Said(party.Party is int count ? $"party of {count}" : "no party");

    [HttpPost("/Requests/Lines")]
    public IActionResult Lines([FromBody] LineNotes lines) =>
        Said($"first line: {lines.Lines?.FirstOrDefault()?.Note ?? "none"}");

    [HttpPost("/Requests/Doses")]
    public IActionResult Doses([FromBody] DoseRecord record) =>
        Said(record.Doses?.TryGetValue(1042, out var dose) == true ? $"dose for 1042: {dose}" : "no dose for 1042");

    [HttpPost("/Requests/Extras")]
    public IActionResult Extras([FromBody] GuideExtras extras) => Said($"guide {extras.Guide?.ToString() ?? "none"}");

    /// <summary>The escort card rendered for the visit's Escort member, its save gathering the request asked for.</summary>
    public IActionResult Escort(EscortGather gather)
    {
        ViewData["Gather"] = gather;
        return View(new EscortPage());
    }

    [HttpPost("/Requests/Escorts")]
    public IActionResult Escorts([FromBody] EscortPage visit) => Said($"escort {visit.Escort.Name ?? "none"}");

    public IActionResult Refused(GatherMistake mistake) => View("Refusals", new GatherRefusalsPage { Mistake = mistake });

    public IActionResult CarePlan() => View(new CarePlanPage());

    /// <summary>Binds what the page read of the chosen file, as any endpoint binds a JSON body.</summary>
    [HttpPost("/Requests/CarePlans/Check")]
    public IActionResult CheckCarePlan([FromBody] CarePlanCheck check) => check.CarePlan is { } file
        ? Said($"{file.Name}, {file.Size} bytes, {file.Type}, changed " +
               DateTimeOffset.FromUnixTimeMilliseconds(file.LastModified).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC")
        : Said("no care plan");

    [HttpPost("/Requests/CarePlans")]
    public async Task<IActionResult> SendCarePlan([FromForm] CarePlanPage page) => Said(await Received(page.CarePlan));

    [HttpGet("/Requests/CarePlans/Lookup")]
    public IActionResult LookUpCarePlan() => Said($"looked up with query {QueryText()}");

    [HttpPost("/Requests/CarePlans/Json")]
    public IActionResult PostCarePlanJson() => Said($"posted as {Request.ContentType ?? "no content"}");

    [HttpPost("/Requests/CarePlans/Sequence")]
    public async Task<IActionResult> SendCarePlanSequence([FromForm] CarePlanUpload upload) => Said(await Received(upload.CarePlan));

    [HttpPost("/Requests/CarePlans/Array")]
    public async Task<IActionResult> SendCarePlanArray([FromForm] CarePlanFiles files) => Said(await Received(files.CarePlan));

    public IActionResult Consents() => View(new ConsentsPage());

    [HttpPost("/Requests/Consents")]
    public async Task<IActionResult> SendConsents([FromForm] ConsentFiles files) => Said(await Received(files.Consents));

    // Each file as the server reads it: its name, its length, its media type and its text.
    private static async Task<string> Received(IEnumerable<IFormFile>? files)
    {
        var received = new List<string>();
        foreach (var file in files ?? [])
        {
            using var reader = new StreamReader(file.OpenReadStream());
            received.Add($"{file.FileName}, {file.Length} bytes of {file.ContentType}: {await reader.ReadToEndAsync()}");
        }

        return received.Count == 0 ? "no file" : "received " + string.Join("; ", received);
    }

    private JsonResult Said(string summary) => Json(new Echo { Summary = summary });

    private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
