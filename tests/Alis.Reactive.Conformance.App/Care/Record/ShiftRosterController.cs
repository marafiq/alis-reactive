using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftRosterController : Controller
{
    private ShiftRosterBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(ShiftRosterBook.Open);

    [HttpGet(CarePages.ShiftRoster)]
    public IActionResult ToApprove() => View("/Care/Record/ShiftRoster.cshtml", Book.Roster);

    [HttpPut(CareEndpoints.ShiftRosterApprovals)]
    public IActionResult Approve([FromForm] Roster roster)
    {
        var validation = this.Validate<Roster, RosterRules>(roster, ShiftRules.Approval);
        if (!validation.IsValid) return validation.ToErrorResponse();

        var approved = Book.Approve(roster.Rows);
        return Content($"Approved shifts {string.Join(", ", approved.Rows.Select(shift => shift.Id))}", "text/plain");
    }
}

internal sealed class ShiftRosterBook : IOwnedByAWorld
{
    private readonly object _gate = new();
    private Roster _roster = new()
    {
        Rows =
        [
            new ShiftRow { Id = 5, Carer = "Ben", Hours = 6 },
            new ShiftRow { Id = 6, Carer = "Ann", Hours = 8 },
            new ShiftRow { Id = 7, Hours = 8 },
        ],
    };

    internal static ShiftRosterBook Open() => new();

    internal Roster Roster
    {
        get
        {
            lock (_gate) return _roster;
        }
    }

    void IOwnedByAWorld.Take(Seed seed)
    {
    }

    internal Roster Approve(List<ShiftRow> rows)
    {
        var approved = new Roster { Rows = rows, Approved = true };
        lock (_gate) _roster = approved;
        return approved;
    }
}

public sealed class Roster
{
    public List<ShiftRow> Rows { get; set; } = [];

    public bool Approved { get; set; }
}

public sealed class ShiftRow
{
    public int Id { get; set; }

    public string? Carer { get; set; }

    public int Hours { get; set; }
}

public sealed class ShiftRules : ReactiveValidator<ShiftRow>
{
    public const string Approval = nameof(Approval);

    public ShiftRules()
    {
        RuleFor(shift => shift.Carer).NotEmpty().WithMessage("Choose a carer.");
        RuleFor(shift => shift.Hours).InclusiveBetween(1, 12).WithMessage("A shift is 1 to 12 hours.");
        RuleSet(Approval, () => RuleFor(shift => shift.Hours).LessThanOrEqualTo(8).WithMessage("An approved shift is 8 hours at most."));
    }
}

public sealed class RosterRules : ReactiveValidator<Roster>
{
    public RosterRules() => RuleSet("default, " + ShiftRules.Approval, () => RuleForEach(roster => roster.Rows).SetValidator(new ShiftRules()));
}
