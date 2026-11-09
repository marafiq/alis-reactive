using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Residents;

public sealed class ResidentsController : Controller
{
    private static readonly IReadOnlyDictionary<string, OnRecord> OnTheRegister = new Dictionary<string, OnRecord>
    {
        ["Helen Park"] = new("12", new WingOnFloor("Rose", 2), [new Medicine("Paracetamol", Controlled: false), new Medicine("Morphine", Controlled: true)]),
        ["Sam Park"] = new("4", new WingOnFloor("Lily", 0), [new Medicine("Paracetamol", Controlled: false)]),
    };

    public IActionResult Lookup() => View(new ResidentLookup { Ward = "South" });

    [HttpGet("/Residents/Summary/{name}")]
    public async Task<IActionResult> Summary(ResidentQuery query)
    {
        Response.Headers["x-register-copy"] = "Main register";
        return query.Ward switch
        {
            "Broken" => await BreakingOff(StatusCodes.Status200OK),
            "Stranded" => await BreakingOff(StatusCodes.Status404NotFound),
            "Annex" => Repairing("the register is being repaired"),
            "Lodge" => Repairing("the register is being repaired", "ask at the front desk"),
            "East" => Content("{\"name\": \"" + query.Name + "\", \"room\": \"12\"}", "text/plain"),
            "North" => Content("{\"name\": \"" + query.Name, "application/json"),
            "Respite" => Content("", "application/json"),
            "Basement" => Dropped(),
            "Relocated" => Relocating(query.Name),
            "Moved" => Redirect("http://" + OtherHost() + ":" + Request.Host.Port + "/Residents/Elsewhere/" + Uri.EscapeDataString(query.Name)),
            "West" => new ContentResult { StatusCode = StatusCodes.Status404NotFound, ContentType = "text/plain", Content = "{\"reason\": \"" + query.Name + " is not on the West register\"}" },
            "Interim" => FromACopy(Answer(query)),
            _ => Answer(query),
        };
    }

    [HttpGet("/Residents/Present/{name}")]
    public IActionResult Present(ResidentQuery query)
        => OnTheRegister.ContainsKey(query.Name) ? Content(query.Name + " is marked present", "text/plain") : StatusCode(StatusCodes.Status500InternalServerError);

    [HttpGet("/Residents/Elsewhere/{name}")]
    public IActionResult Elsewhere(string name)
    {
        Response.Headers.AccessControlAllowOrigin = "*";
        Response.Headers.AccessControlExposeHeaders = "X-Register-Copy";
        Response.Headers["X-Register-Copy"] = "Elsewhere register";
        Response.Headers["X-Register-Note"] = "kept at the annex";
        return Answer(new ResidentQuery { Name = name, Ward = "Moved" });
    }

    [HttpOptions("/Residents/Elsewhere/{name}")]
    public IActionResult ElsewhereAllows()
    {
        Response.Headers.AccessControlAllowOrigin = "*";
        Response.Headers.AccessControlAllowHeaders = "*";
        return NoContent();
    }

    [HttpGet("/Residents/Kin/{name}")]
    public IActionResult Kin(ResidentQuery query) => Json(new KinAnswer(query.Name, query.NextOfKin.Name));

    public IActionResult NextOfKin() => View(new ResidentLookup());

    [HttpGet("/Residents/Register/{ward}")]
    public IActionResult Register(WardQuery query)
        => query.Ward == "Annex" ? StatusCode(StatusCodes.Status500InternalServerError) : Json(new RegisterOpen(query.Ward));

    public IActionResult PlaceholderNoMemberFills() => View(new ResidentLookup());

    public IActionResult UnpairedBrace() => View(new ResidentLookup());

    public IActionResult Fragment() => View(new ResidentLookup());

    public IActionResult MemberSetTwice() => View(new ResidentLookup());

    public IActionResult PathWithNoPlaceholder() => View(new ResidentLookup());

    public IActionResult PathOfANestedMember() => View(new ResidentLookup());

    public IActionResult QueryOfAPlaceholder() => View(new ResidentLookup());

    public IActionResult MemberNotAPath() => View(new ResidentLookup());

    public IActionResult HeaderNamed(string name, string value) => View(new HeaderAsk { Name = name, Value = value });

    public IActionResult ReplyHeaderNamed(string name) => View(new HeaderAsk { Name = name });

    public IActionResult RouteAfterAStep() => View(new ResidentLookup());

    public IActionResult RequestWrittenTwice() => View(new ResidentLookup());

    public IActionResult RoutedRequestWrittenTwice() => View(new ResidentLookup());

    public IActionResult StatusTwice() => View(new ResidentLookup());

    public IActionResult StatusNotAnError() => View(new ResidentLookup());

    public IActionResult CatchAllTwice() => View(new ResidentLookup());

    public IActionResult ReplyAfterItsRequest() => View(new ResidentLookup());

    public IActionResult BodyKeptOutOfItsRoute() => View(new ResidentLookup());

    public IActionResult HeadersKeptOutOfItsRoute() => View(new ResidentLookup());

    public IActionResult ReasonKeptOutOfItsRoute() => View(new ResidentLookup());

    public IActionResult StatusKeptOutOfTheCatchAll() => View(new ResidentLookup());

    private StatusCodeResult Repairing(params string[] notes)
    {
        Response.Headers.Append("X-Register-Note", notes);
        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    private RedirectResult Relocating(string name)
    {
        Response.Headers["x-register-copy"] = "Relocating desk";
        return Redirect("/Residents/Summary/" + Uri.EscapeDataString(name) + "?ward=South");
    }

    private async Task<EmptyResult> BreakingOff(int status)
    {
        Response.StatusCode = status;
        Response.ContentType = "application/json";
        Response.ContentLength = 1000;
        await Response.WriteAsync("{\"reason\": \"The register stopped");
        await Response.Body.FlushAsync();
        return new EmptyResult();
    }

    private EmptyResult Dropped()
    {
        HttpContext.Abort();
        return new EmptyResult();
    }

    private static IActionResult FromACopy(IActionResult answer)
    {
        if (answer is JsonResult found) found.StatusCode = StatusCodes.Status203NonAuthoritative;
        return answer;
    }

    private string OtherHost() => Request.Host.Host == "localhost" ? "127.0.0.1" : "localhost";

    private IActionResult Answer(ResidentQuery query)
    {
        if (!OnTheRegister.TryGetValue(query.Name, out var resident)) return NotFound(new NoResident("No resident is named " + query.Name));
        return Json(new ResidentSummary(query.Name, resident.Room, query.Ward, query.Home, resident.Wing, resident.Medicines));
    }

    private sealed record OnRecord(string Room, WingOnFloor Wing, IReadOnlyList<Medicine> Medicines);
}

public sealed class ResidentLookup
{
    public string Name { get; set; } = "";

    public string Ward { get; set; } = "";

    public string NextOfKin { get; set; } = "";
}

public sealed class HeaderAsk
{
    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}

public sealed class ResidentQuery
{
    public string Name { get; set; } = "";

    public string Ward { get; set; } = "";

    public string Home { get; set; } = "";

    public Kin NextOfKin { get; set; } = new();

    public sealed class Kin
    {
        public string Name { get; set; } = "";
    }
}

public sealed class WardQuery
{
    public string Ward { get; set; } = "";
}

public sealed record ResidentSummary(string Name, string Room, string? Ward, string? Home, WingOnFloor Wing, IReadOnlyList<Medicine> Medicines);

public sealed record WingOnFloor(string Name, int Floor);

public sealed record Medicine(string Name, bool Controlled);

public sealed record NoResident(string Reason);

public sealed record RegisterOpen(string Ward);

public sealed record KinAnswer(string Name, string Kin);
