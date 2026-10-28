using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Arrays;

/// <summary>Arrays read from a source, filtered, projected, ordered, asked for one value, and used where a value is read.</summary>
public sealed class ArraysController : Controller
{
    public IActionResult Sources() => View(new SourcesPage());

    public IActionResult Filters() => View(new FiltersPage());

    public IActionResult Projections() => View(new ProjectionsPage());

    public IActionResult Orders() => View(new OrdersPage());

    public IActionResult Questions() => View(new QuestionsPage());

    public IActionResult Results() => View(new ResultsPage());

    /// <summary>A page that makes the mistake named, refused when it renders.</summary>
    public IActionResult Refused(Mistake mistake) => View("Refusals", new RefusalsPage { Mistake = mistake });

    /// <summary>The residents in the order the home keeps them, as any endpoint answers a list.</summary>
    [HttpGet]
    public IActionResult Residents() => Json(Register.Residents());

    /// <summary>The census: the residents as a sequence, and no list of visitors.</summary>
    [HttpGet]
    public IActionResult Census() => Json(new Census { Residents = Register.Residents(), Visitors = null });

    /// <summary>Binds the posted notice as any endpoint does, and says whom it is about.</summary>
    [HttpPost]
    public IActionResult Notify([FromBody] Notice notice) =>
        Json(new NoticeEcho { Summary = $"{notice.Names.Count} named: {string.Join(" and ", notice.Names)}" });
}
