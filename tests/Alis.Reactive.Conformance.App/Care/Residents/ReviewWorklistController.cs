using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ReviewWorklistController : Controller
{
    [HttpGet(CarePages.ReviewWorklist)]
    public IActionResult Worklist() => View("/Care/Residents/ReviewWorklist.cshtml", new ReviewWorklist());
}

public sealed class ReviewWorklist
{
    public IReadOnlyList<ReviewResident> Residents { get; } =
    [
        new(101, "Ada Brooks", "North", 7, false),
        new(102, "Ben Ortiz", "North", 2, true),
        new(103, "Cora Bell", "North", 6, false),
        new(104, "Dev Singh", "South", 11, false),
        new(105, "Eve Moss", "South", 2, true),
        new(106, "Finn Ward", "South", 6, false)
    ];

    public ReviewResident Referral { get; } = new(107, "Inez Shaw", "South", 9, false);
}

public sealed record ReviewResident(int Reference, string Name, string Wing, int DaysOverdue, bool Escalated);
