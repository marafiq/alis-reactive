using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MealCardController : Controller
{
    [HttpGet(CarePages.MealCard)]
    public IActionResult OfAResident() => View("/Care/Record/MealCard.cshtml", new MealCard("Ada Hart"));

    [HttpGet(MealCardEndpoints.Menu)]
    public IActionResult Menu() => PartialView("/Care/Record/MealCardMenu.cshtml", new MealCardMenu("Leek soup, then baked cod."));
}

public sealed record MealCard(string Resident);

public sealed record MealCardMenu(string Dishes);

public static class MealCardEndpoints
{
    private const string Root = CarePages.Area + "record/meal-card/";

    public const string Menu = Root + "menu";
}

public static class MealCards
{
    public static readonly AppEvent OpenTheMenu = AppEvent.Declare();

    public static readonly AppEvent AddANote = AppEvent.Declare();

    public static readonly AppEvent CloseTheMenu = AppEvent.Declare();
}
