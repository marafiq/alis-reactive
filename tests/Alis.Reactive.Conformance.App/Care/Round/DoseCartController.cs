using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseCartController : Controller
{
    [HttpGet(CarePages.DoseCartWithACopayOfOneTenth)]
    public IActionResult WithACopayOfOneTenth() => View("/Care/Round/DoseCart.cshtml", new DoseCart("North", DoseCartEndpoints.CartWithACopayOfOneTenth));

    [HttpGet(CarePages.DoseCartWithACopayOfOneEighth)]
    public IActionResult WithACopayOfOneEighth() => View("/Care/Round/DoseCart.cshtml", new DoseCart("North", DoseCartEndpoints.CartWithACopayOfOneEighth));

    [HttpGet(CarePages.DoseCartWithACopayOfTenHundredths)]
    public IActionResult WithACopayOfTenHundredths() => View("/Care/Round/DoseCart.cshtml", new DoseCart("North", DoseCartEndpoints.CartWithACopayOfTenHundredths));

    [HttpGet(DoseCartEndpoints.CartWithACopayOfOneTenth)]
    public IActionResult CartWithACopayOfOneTenth() => Json(TheCartWhoseAspirinCosts(0.1m));

    [HttpGet(DoseCartEndpoints.CartWithACopayOfOneEighth)]
    public IActionResult CartWithACopayOfOneEighth() => Json(TheCartWhoseAspirinCosts(0.125m));

    [HttpGet(DoseCartEndpoints.CartWithACopayOfTenHundredths)]
    public IActionResult CartWithACopayOfTenHundredths() => Json(TheCartWhoseAspirinCosts(0.10m));

    private static CartContents TheCartWhoseAspirinCosts(decimal copay) => new()
    {
        Doses =
        [
            new CartDose { Medicine = "Digoxin", Copay = 2.50m },
            new CartDose { Medicine = "Aspirin", Copay = copay },
        ],
    };
}

public sealed record DoseCart(string Ward, string Cart);

public sealed class CartContents
{
    public List<CartDose> Doses { get; init; } = [];
}

public sealed class CartDose
{
    public string Medicine { get; init; } = "";

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public decimal Copay { get; init; }
}

public static class DoseCartEndpoints
{
    private const string Root = CarePages.Area + "round/dose-cart/";
    public const string CartWithACopayOfOneTenth = Root + "cart/copay-of-one-tenth";
    public const string CartWithACopayOfOneEighth = Root + "cart/copay-of-one-eighth";
    public const string CartWithACopayOfTenHundredths = Root + "cart/copay-of-ten-hundredths";
}

public static class DoseCarts
{
    public static readonly AppEvent CheckThePrice = AppEvent.Declare();
    public static readonly AppEvent CheckTheNote = AppEvent.Declare();
}
