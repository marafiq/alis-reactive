using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Arrays;

public sealed class ArraysController : Controller
{
    public IActionResult Sources() => View(new SourcesPage());

    public IActionResult Filters() => View(new FiltersPage());

    public IActionResult Projections() => View(new ProjectionsPage());

    public IActionResult Orders() => View(new OrdersPage());

    public IActionResult Questions() => View(new QuestionsPage());

    public IActionResult Results() => View(new ResultsPage());

    public IActionResult Refused(Mistake mistake) => View("Refusals", new RefusalsPage { Mistake = mistake });

    [HttpGet]
    public IActionResult Residents() => Json(Register.Residents());

    [HttpGet]
    public IActionResult Census() => Json(new Census { Residents = Register.Residents(), Visitors = null });

    [HttpPost]
    public IActionResult Notify([FromBody] Notice notice)
        => Json(new NoticeEcho { Summary = $"{notice.Names.Count} named: {string.Join(" and ", notice.Names)}" });
}
