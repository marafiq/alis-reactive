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
    public IActionResult Summary(ResidentQuery query) => query.Ward switch
    {
        "Annex" => StatusCode(StatusCodes.Status500InternalServerError),
        "East" => Content("{\"name\": \"" + query.Name + "\", \"room\": \"12\"}", "text/plain"),
        "North" => Content("{\"name\": \"" + query.Name, "application/json"),
        "Respite" => Content("", "application/json"),
        "Basement" => Dropped(),
        "Moved" => Redirect("http://" + OtherHost() + ":" + Request.Host.Port + "/Residents/Elsewhere/" + Uri.EscapeDataString(query.Name)),
        "West" => new ContentResult { StatusCode = StatusCodes.Status404NotFound, ContentType = "text/plain", Content = "{\"reason\": \"" + query.Name + " is not on the West register\"}" },
        _ => Answer(query),
    };

    [HttpGet("/Residents/Elsewhere/{name}")]
    public IActionResult Elsewhere(string name)
    {
        Response.Headers.AccessControlAllowOrigin = "*";
        return Answer(new ResidentQuery { Name = name, Ward = "Moved" });
    }

    [HttpGet("/Residents/Register/{ward}")]
    public IActionResult Register(WardQuery query)
        => query.Ward == "Annex" ? StatusCode(StatusCodes.Status500InternalServerError) : Json(new RegisterOpen(query.Ward));

    public IActionResult PlaceholderNoMemberFills() => View(new ResidentLookup());

    public IActionResult UnpairedBrace() => View(new ResidentLookup());

    public IActionResult Fragment() => View(new ResidentLookup());

    public IActionResult MemberSetTwice() => View(new ResidentLookup());

    public IActionResult NestedMember() => View(new ResidentLookup());

    public IActionResult ResponseAfterAStep() => View(new ResidentLookup());

    public IActionResult ResponseTwice() => View(new ResidentLookup());

    public IActionResult SuccessTwice() => View(new ResidentLookup());

    public IActionResult StatusTwice() => View(new ResidentLookup());

    public IActionResult StatusNotAnError() => View(new ResidentLookup());

    public IActionResult CatchAllTwice() => View(new ResidentLookup());

    private EmptyResult Dropped()
    {
        HttpContext.Abort();
        return new EmptyResult();
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
