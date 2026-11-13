using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LunchFeedbackController : Controller
{
    private const string Feedback = "/Care/RecordContacts/LunchFeedback.cshtml";

    [HttpGet(CarePages.LunchRatedFour)]
    public IActionResult RatedFour() => View(Feedback, new LunchFeedback { LunchRating = 4 });

    [HttpGet(CarePages.LunchRatedTwo)]
    public IActionResult RatedTwo() => View(Feedback, new LunchFeedback { LunchRating = 2 });
}

public sealed class LunchFeedback
{
    public double LunchRating { get; set; }
}
