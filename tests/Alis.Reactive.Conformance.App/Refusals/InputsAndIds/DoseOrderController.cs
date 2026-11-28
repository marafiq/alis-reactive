using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DoseOrderController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.DoseOrderCountOfItems)]
    public IActionResult CountOfItems() => View("/Refusals/InputsAndIds/DoseOrderCountOfItems.cshtml", AnOrderOfOneItem());

    [HttpGet(RefusalPages.DoseOrderFirstItemsName)]
    public IActionResult FirstItemsName() => View("/Refusals/InputsAndIds/DoseOrderFirstItemsName.cshtml", AnOrderOfOneItem());

    [HttpPost(RefusalEndpoints.DoseOrders)]
    public IActionResult Save([FromForm] DoseOrder order)
        => Content(order.Items is [{ Name: { } name }, ..] ? $"First item: {name}" : "No first item", PlainText);

    private static DoseOrder AnOrderOfOneItem() => new() { Items = [new Item()] };
}

public sealed class DoseOrder
{
    public List<Item> Items { get; set; } = [];
}

public sealed class Item
{
    public string? Name { get; set; }
}

public static class DoseOrders
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
