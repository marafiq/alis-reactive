using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.AvailabilityPanel)]
    public IActionResult AvailabilityPanel([FromQuery] AvailabilityQuery query)
    {
        if (query.BedId.Length == 0) return Content("Choose a bed to open availability.");
        var status = Desk.AvailabilityAccess(query.DraftId, query.CommunityId);
        if (status != 200) return AvailabilityError(status);
        return PartialView(Views + "AvailabilityPanel.cshtml", new AvailabilityPanel(query.DraftId, query.CommunityId, query.BedId, Desk.PageOf(query.DraftId)!.Payer.Kind));
    }

    [HttpGet(MoveInEndpoints.AvailabilityCard)]
    public IActionResult AvailabilityCard([FromQuery] AvailabilityQuery query)
    {
        if (query.BedId.Length == 0) return Content("Choose a bed to open its card.");
        var status = Desk.AvailabilityAccess(query.DraftId, query.CommunityId);
        if (status != 200) return AvailabilityError(status);
        return PartialView(Views + "AvailabilityCard.cshtml", new AvailabilityCard(query.DraftId, query.CommunityId, query.BedId));
    }

    [HttpGet(MoveInEndpoints.Availability)]
    public async Task<IActionResult> Availability([FromQuery] AvailabilityQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (query.DraftId.Length == 0) return Content("Choose a move-in to read bed availability.");
        var snapshot = Desk.AvailabilityOf(query.CommunityId, query.BedId);
        await World.Reach(GateName.Of(RealtimeGate.Answer), Request.Path);
        var status = Desk.AvailabilityAccess(query.DraftId, query.CommunityId);
        if (status != 200) return AvailabilityError(status);
        if (snapshot is null) return NotFound(MoveInErrors.Missing());
        if (snapshot.Reply.Revision < query.SinceRevision) return StatusCode(StatusCodes.Status503ServiceUnavailable, MoveInErrors.Refused("revision-pending", "The requested bed revision is unavailable."));
        Response.Headers[MoveInEndpoints.BedHolderHeader] = snapshot.Holder;
        Response.Headers[MoveInEndpoints.BedStateHeader] = snapshot.State.ToString();
        return Json(snapshot.Reply);
    }

    [HttpGet(MoveInEndpoints.Services)]
    public IActionResult ServiceCatalogue([FromQuery] ServiceCatalogueQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (query.DraftId.Length == 0) return Content("Choose a move-in to read the service catalogue.");
        if (!ModelState.IsValid || !Enum.IsDefined(query.CareLevel) || !Enum.IsDefined(query.PayerKind)) return BadRequest();
        var (status, catalogue) = Desk.ServiceCatalogueOf(query.DraftId);
        return status == 200 ? Json(catalogue) : AvailabilityError(status);
    }

    [HttpGet(MoveInEndpoints.AvailabilityStream)]
    public async Task<IResult> AvailabilityStream([FromQuery] string? draftId, [FromQuery] string? communityId, [FromQuery] bool card, CancellationToken stopped)
    {
        if (draftId is null || communityId is null) return Results.Text("Choose a move-in and community to open live availability.");
        var stream = World.Streams.Of(BedAvailabilityFeed.Address(draftId, communityId, card));
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var access = Desk.AvailabilityAccess(draftId, communityId);
        if (access != 200) return Results.StatusCode(access);
        Desk.PublishAvailabilityTo(draftId, communityId, stream);
        return ServerEvents.Write(HttpContext, AvailabilityMessages(accepted, stream.Opens(stopped), stopped));
    }

    private static async IAsyncEnumerable<ServerMessage> AvailabilityMessages(int accepted, ChannelReader<Pushed> pushed, [EnumeratorCancellation] CancellationToken stopped)
    {
        var feed = new BedAvailabilityFeed();
        yield return feed.Opened.Of(new BedFeedOpened(accepted));
        await foreach (var message in pushed.ReadAllAsync(stopped))
        {
            var changed = JsonSerializer.Deserialize<BedChanged>(message.Data, JsonSerializerOptions.Web) ?? throw new InvalidOperationException("A bed change did not carry a bed.");
            yield return feed.BedChanged.Of(changed);
        }
    }

    private IActionResult AvailabilityError(int status)
        => status == 403 ? StatusCode(status, MoveInErrors.Forbidden()) : NotFound(MoveInErrors.Missing());
}

public static partial class MoveInEndpoints
{
    public const string AvailabilityPanel = CarePages.MoveIns + "availability-panel";
    public const string AvailabilityCard = CarePages.MoveIns + "availability-card";
    public const string Availability = CareEndpoints.MoveInAvailability;
    public const string AvailabilityStream = CareEndpoints.MoveInBedFeed;
    public const string Services = CareEndpoints.MoveInServices;
    public const string BedHolderHeader = "X-Bed-Holder";
    public const string BedStateHeader = "X-Bed-State";
}
