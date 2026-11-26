using System.Text.Json.Serialization;
using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class KinContactsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(RefusalPages.KinContactsNamedPastTheirCase)]
    public ActionResult NamedPastTheirCase() => View("/Refusals/RequestsAndResponses/KinContactsNamedPastTheirCase.Net48.cshtml", new KinDesk("Edith"));

    [HttpGet]
    [Address(RefusalPages.KinContactsNamedByTheirCase)]
    public ActionResult NamedByTheirCase() => View("/Refusals/RequestsAndResponses/KinContactsNamedByTheirCase.Net48.cshtml", new KinDesk("Edith"));

    [HttpPost]
    [Address(KinContactsEndpoints.PastTheirCase)]
    public ActionResult SavePastTheirCase(KinContactsPastTheirCase saved) => Content(saved.Daughter.GivenName ?? "no given name", PlainText);

    [HttpPost]
    [Address(KinContactsEndpoints.ByTheirCase)]
    public ActionResult SaveByTheirCase(KinContactsByTheirCase saved) => Content(saved.Daughter.GivenName ?? "no given name", PlainText);
}

public sealed record KinDesk(string Resident);

public sealed class KinContactsPastTheirCase
{
    public KinContactPastItsCase Daughter { get; set; } = new();
}

public sealed class KinContactPastItsCase
{
    [JsonPropertyName("given")]
    public string? GivenName { get; set; }
}

public sealed class KinContactsByTheirCase
{
    public KinContactByItsCase Daughter { get; set; } = new();
}

public sealed class KinContactByItsCase
{
    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }
}

public static class KinContactsEndpoints
{
    public const string PastTheirCase = RefusalPages.KinContactsNamedPastTheirCase + "/contacts";
    public const string ByTheirCase = RefusalPages.KinContactsNamedByTheirCase + "/contacts";
}
