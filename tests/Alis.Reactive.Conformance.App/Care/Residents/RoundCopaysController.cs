using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundCopaysController : Controller
{
    private const string JsonText = "application/json";

    private const string FiveDosesOneCopayUnread =
        """[{"medicine":"Aspirin","billing":{"copay":"0.10"}},{"medicine":"Digoxin","billing":{"copay":"0.1"}},{"medicine":"Insulin","billing":{"copay":"0.1000000000000000000000000001"}},{"medicine":"Warfarin","billing":null},{"medicine":"Heparin","billing":{"copay":"abc"}}]""";

    [HttpGet(CarePages.RoundCopays)]
    public IActionResult EveryCopayRead() => View("/Care/Residents/RoundCopays.cshtml", new CopayRound(RoundCopayEndpoints.Doses));

    [HttpGet(CarePages.RoundCopaysWithOneUnread)]
    public IActionResult OneCopayUnread() => View("/Care/Residents/RoundCopays.cshtml", new CopayRound(RoundCopayEndpoints.DosesWithOneCopayUnread));

    [HttpGet(RoundCopayEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<CopayDose>
    {
        new("Aspirin", new CopayBilling { Copay = 0.10m }),
        new("Digoxin", new CopayBilling { Copay = 0.1m }),
        new("Insulin", new CopayBilling { Copay = 0.1000000000000000000000000001m }),
        new("Warfarin", null),
    });

    [HttpGet(RoundCopayEndpoints.DosesWithOneCopayUnread)]
    public IActionResult DosesWithOneCopayUnread() => Content(FiveDosesOneCopayUnread, JsonText);
}

public sealed record CopayRound(string Doses);

public sealed record CopayDose(string Medicine, CopayBilling? Billing);

public sealed class CopayBilling
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public decimal Copay { get; init; }
}

public static class RoundCopayEndpoints
{
    private const string Root = CarePages.Area + "residents/round-copays/";

    public const string Doses = Root + "doses";

    public const string DosesWithOneCopayUnread = Root + "doses-with-one-copay-unread";
}
