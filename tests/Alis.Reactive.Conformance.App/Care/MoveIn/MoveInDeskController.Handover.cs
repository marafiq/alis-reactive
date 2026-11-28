using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CarePages.AdmissionHandover)]
    public IActionResult AdmissionHandover()
        => Desk.Handover(HelensDraft) is { } page ? View(Views + "AdmissionHandover.cshtml", page) : NotFound();

    [HttpGet(CareEndpoints.AdmissionFilingTray)]
    public IActionResult FilingTray([FromQuery] DeskQuery query)
        => ReviewAccess(query.DraftId) ?? PartialView(Views + "FilingTray.cshtml", Desk.PageOf(query.DraftId));

    [HttpGet(CarePages.AdmissionReception)]
    public IActionResult VisitorReception()
        => Desk.ReceptionOf(HelensDraft) is { } reception ? View(Views + "ReceptionDesk.cshtml", reception) : NotFound();

    [HttpGet(CarePages.AdmissionInformation)]
    public IActionResult AdmissionInformation() => View(Views + "AdmissionInformation.cshtml", new AdmissionInformation());

    [HttpGet(CarePages.MoveIns + "catalogue-directory")]
    [HttpOptions(CarePages.MoveIns + "catalogue-directory")]
    public IActionResult CatalogueDirectory()
    {
        Response.Headers.AccessControlAllowOrigin = $"{Request.Scheme}://127.0.0.1:{Request.Host.Port}";
        Response.Headers.AccessControlAllowHeaders = "Care-Site";
        Response.Headers.AccessControlAllowMethods = "GET";
        return Content("Community services directory");
    }

    [HttpGet(CareEndpoints.AdmissionDeskStatus)]
    public async Task<IActionResult> DeskStatus()
    {
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return World.NextAnswer(Request.Path) == 503 ? StatusCode(503) : Content("Admission desk available");
    }
}

public sealed class AdmissionInformation;
