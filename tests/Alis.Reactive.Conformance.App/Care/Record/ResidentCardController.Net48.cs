using System.Text.Json.Serialization;
using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentCardController : Controller
{
    private const string CardView = "/Care/Record/ResidentCard.Net48.cshtml";
    private const string DetailsView = "/Care/Record/ResidentCardDetails.Net48.cshtml";

    [HttpGet]
    [Address(CarePages.ResidentCard)]
    public ActionResult OfTheResident() => View(CardView, new ResidentCard(ResidentCardEndpoints.DetailsTab));

    [HttpGet]
    [Address(CarePages.ResidentCardAnsweredByJsonResult)]
    public ActionResult AnsweredByJsonResult() => View(CardView, new ResidentCard(ResidentCardEndpoints.JsonResultDetailsTab));

    [HttpGet]
    [Address(ResidentCardEndpoints.DetailsTab)]
    public ActionResult Details() => PartialView(DetailsView, new ResidentCardDetails(ResidentCardEndpoints.ResidentSeven));

    [HttpGet]
    [Address(ResidentCardEndpoints.JsonResultDetailsTab)]
    public ActionResult DetailsAnsweredByJsonResult() => PartialView(DetailsView, new ResidentCardDetails(ResidentCardEndpoints.JsonResultResidentSeven));

    [HttpGet]
    [Address(ResidentCardEndpoints.ResidentSeven)]
    public ActionResult ResidentSeven() => this.JsonResponse(ResidentSevensDetails());

    [HttpGet]
    [Address(ResidentCardEndpoints.JsonResultResidentSeven)]
    public ActionResult ResidentSevenByJsonResult() => Json(ResidentSevensDetails(), JsonRequestBehavior.AllowGet);

    private ResidentDetails ResidentSevensDetails()
        => new(TwinWorlds.Of(HttpContext).DateOfBirth ?? throw new InvalidOperationException("The world holds no date of birth for resident 7: a test seeds it with Seed.TheResidentBornOn."));
}

public sealed record ResidentCard(string DetailsTab);

public sealed record ResidentCardDetails(string Resident);

public sealed record ResidentDetails([property: JsonPropertyName("DateOfBirth")] DateTime DateOfBirth);

public static class ResidentCardEndpoints
{
    public const string DetailsTab = CarePages.ResidentCard + "/details";
    public const string JsonResultDetailsTab = CarePages.ResidentCardAnsweredByJsonResult + "/details";
    public const string ResidentSeven = CarePages.ResidentCard + "/residents/7";
    public const string JsonResultResidentSeven = CarePages.ResidentCardAnsweredByJsonResult + "/residents/7";
}

public static class ResidentCardButtons
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
