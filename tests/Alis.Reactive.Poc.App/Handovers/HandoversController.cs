using System.Globalization;
using Alis.Reactive.Poc.App.Components;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Handovers;

public sealed class HandoversController : Controller
{
    public IActionResult Ask() => View(new HandoverPage());

    [HttpGet("/Handovers/Register")]
    public IActionResult Register(RegisterQuery query) => Json(new RegisterAnswer("Round " + query.Round.ToString(CultureInfo.InvariantCulture) + " is on the register"));

    [HttpGet("/Handovers/Round/{round}/{due}/{shift}")]
    public IActionResult Round(RoundQuery query) => Json(new RegisterAnswer("Round " + query.Round.ToString(CultureInfo.InvariantCulture) + " opened for the " + query.Shift + " shift"));

    [HttpGet("/Handovers/Check")]
    public IActionResult Check(HandoverCheck check) => Json(new HandoverChecked(
        Number(check.Round),
        Moment(check.StartsAt),
        check.EndsAt is { } ends ? Moment(ends) : "none",
        check.Beds.Count == 0 ? "none" : string.Join(", ", check.Beds),
        check.Lead ?? "none",
        check.Target is { } target ? Number(target) : "none",
        ModelState.IsValid ? "none" : string.Join(", ", ModelState.Where(entry => entry.Value?.Errors.Count > 0).Select(entry => entry.Key).Order(StringComparer.Ordinal))));

    public IActionResult DateInPath() => View(new HandoverPage());

    public IActionResult InstantInPath() => View(new HandoverPage());

    public IActionResult ListInPath() => View(new HandoverPage());

    public IActionResult ObjectMember() => View(new HandoverPage());

    public IActionResult DictionaryMember() => View(new HandoverPage());

    public IActionResult ListOfLists() => View(new HandoverPage());

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Moment(DateTimeOffset moment) => moment.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
}

public sealed class HandoverPage
{
}

public sealed class RegisterQuery
{
    public int Round { get; set; }

    public decimal Dose { get; set; }

    public double Rate { get; set; }

    public bool Due { get; set; }

    public string Ward { get; set; } = "";

    public DateTime PlannedFor { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public List<int> Beds { get; set; } = [];

    public List<int> Late { get; set; } = [];

    public List<int?> Waiting { get; set; } = [];

    public List<string?> Leads { get; set; } = [];

    public Shift Shift { get; set; }

    public Shift Cover { get; set; }

    public int? Target { get; set; }

    public Handover? Handover { get; set; }

    public Dictionary<string, int> Counts { get; set; } = [];

    public List<List<int>> Bays { get; set; } = [];
}

public sealed class RoundQuery
{
    public int Round { get; set; }

    public bool Due { get; set; }

    public Shift Shift { get; set; }
}

public sealed class HandoverCheck
{
    public int Round { get; set; } = 1;

    public DateTimeOffset StartsAt { get; set; } = new(2026, 11, 9, 6, 0, 0, TimeSpan.Zero);

    public DateTimeOffset? EndsAt { get; set; } = new(2026, 11, 9, 18, 0, 0, TimeSpan.Zero);

    public List<int> Beds { get; set; } = [1];

    public string? Lead { get; set; } = "the day lead";

    public int? Target { get; set; } = 2000;
}

public sealed record RegisterAnswer(string Recorded);

public sealed record HandoverChecked(string Round, string StartsAt, string EndsAt, string Beds, string Lead, string Target, string Problems);
