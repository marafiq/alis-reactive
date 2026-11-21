using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentSearchController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ResidentSearch)]
    public IActionResult Search() => View("/Care/Residents/ResidentSearch.cshtml", new ResidentSearch());

    [HttpGet(ResidentSearchEndpoints.SearchedTerm)]
    public IActionResult SearchedTerm(ResidentSearchQuery query) => Content(query.Term ?? "", PlainText);
}

public sealed class ResidentSearch
{
    public string? Term { get; set; }
}

public sealed class ResidentSearchQuery
{
    public string? Term { get; set; }
}

public static class ResidentSearches
{
    public static readonly AppEvent Search = AppEvent.Declare();
}

public static class ResidentSearchEndpoints
{
    public const string SearchedTerm = CarePages.Area + "residents/search/searched-term";
}
