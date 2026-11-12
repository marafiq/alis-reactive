using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StaffRosterController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.StaffRosterWithAnOpenShift)]
    public IActionResult WithAnOpenShift() => View("/Care/Round/StaffRoster.cshtml", new StaffRoster("North", StaffRosterEndpoints.PlannedWithAnOpenShift));

    [HttpGet(CarePages.StaffRosterWithEveryShiftFilled)]
    public IActionResult WithEveryShiftFilled() => View("/Care/Round/StaffRoster.cshtml", new StaffRoster("North", StaffRosterEndpoints.PlannedWithEveryShiftFilled));

    [HttpGet(StaffRosterEndpoints.PlannedWithAnOpenShift)]
    public IActionResult PlannedWithAnOpenShift() => Json(new List<Shift?> { Shift.Day, null, Shift.Night });

    [HttpGet(StaffRosterEndpoints.PlannedWithEveryShiftFilled)]
    public IActionResult PlannedWithEveryShiftFilled() => Json(new List<Shift?> { Shift.Day, Shift.Night });

    [HttpGet(StaffRosterEndpoints.Sent)]
    public IActionResult Sent(SentRoster roster)
        => Content($"{roster.Shifts.Count} items: {string.Join(", ", roster.Shifts.Select(shift => shift?.ToString() ?? "none"))}", PlainText);
}

public sealed record StaffRoster(string Ward, string Planned);

public sealed class SentRoster
{
    public List<Shift?> Shifts { get; set; } = [];
}

public enum Shift
{
    Day,
    Night,
}

public static class StaffRosterEndpoints
{
    private const string Root = CarePages.Area + "round/staff-roster/";
    public const string PlannedWithAnOpenShift = Root + "planned/an-open-shift";
    public const string PlannedWithEveryShiftFilled = Root + "planned/every-shift-filled";
    public const string Sent = Root + "sent";
}

public static class StaffRosters
{
    public static readonly AppEvent Send = AppEvent.Declare();
}
