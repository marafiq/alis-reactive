using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Conditions;

public sealed class ConditionsController : Controller
{
    public IActionResult Levels() => View(new LevelsPage());

    public IActionResult Shifts() => View(new ShiftsPage());

    public IActionResult SpanOrder() => View(new RefusedPage());

    public IActionResult Goal() => View(new GoalPage());

    public IActionResult Names() => View(new NamesPage());

    public IActionResult Round() => View(new RoundPage());

    public IActionResult Visit() => View(new VisitPage());

    public IActionResult Stay() => View(new StayPage());

    public IActionResult Triage() => View(new TriagePage());

    public IActionResult Groups() => View(new TriagePage());

    public IActionResult CarePlan() => View(new CarePlanPage());

    public IActionResult Availability() => View(new AvailabilityPage());

    public IActionResult Decisions() => View(new DecisionsPage());

    public IActionResult ElseIfAfterElse() => View(new RefusedPage());

    public IActionResult ElseTwice() => View(new RefusedPage());

    public IActionResult NegativeMinLength() => View(new NamesPage());
}
