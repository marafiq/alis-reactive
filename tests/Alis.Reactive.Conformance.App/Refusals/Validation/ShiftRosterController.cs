using System.Collections.Generic;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ShiftRosterController : Controller
{
    [HttpGet(RefusalPages.ShiftRosterApprovedWithNoRowRule)]
    public IActionResult ApprovedWithNoRowRule() => View("/Refusals/Validation/ShiftRoster.cshtml", new Roster
    {
        Rows =
        [
            new ShiftRow { Id = 5, Carer = "Ben", Hours = 6 },
            new ShiftRow { Id = 6, Carer = "Ann", Hours = 8 },
            new ShiftRow { Id = 7, Hours = 8 },
        ],
    });
}

public sealed class Roster
{
    public List<ShiftRow> Rows { get; set; } = [];
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
    public RosterRules() => RuleForEach(roster => roster.Rows).SetValidator(new ShiftRules());
}

public static class ShiftRosterEndpoints
{
    public const string Approvals = RefusalPages.ShiftRosterApprovedWithNoRowRule + "/approvals";
}
