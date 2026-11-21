using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class KinDeskController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.KinDesk)]
    public IActionResult CheckedInTheBrowser() => View("/Care/Admission/KinDesk.cshtml", new KinDesk());

    [HttpGet(CarePages.KinDeskCheckedOnTheServer)]
    public IActionResult CheckedOnTheServer() => View("/Care/Admission/KinDeskCheckedOnTheServer.cshtml", new ServerCheckedKinDesk());

    [HttpPost(CareEndpoints.KinDeskDesks)]
    public IActionResult Save([FromForm] KinDesk desk)
    {
        var validation = this.Validate<KinDesk, KinDeskRules>(desk);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content($"next of kin {desk.NextOfKin.Name}", PlainText);
    }

    [HttpPost(CareEndpoints.ServerCheckedKinDeskDesks)]
    public IActionResult SaveCheckedOnTheServer([FromForm] ServerCheckedKinDesk desk)
    {
        var validation = this.Validate<ServerCheckedKinDesk, ServerCheckedKinDeskRules>(desk);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content($"next of kin {desk.NextOfKin.Name}", PlainText);
    }
}

public sealed class KinDesk
{
    public DeskKin NextOfKin { get; set; } = new();
}

public sealed class ServerCheckedKinDesk
{
    public DeskKin NextOfKin { get; set; } = new();
}

public sealed class DeskKin
{
    public string? Name { get; set; }
}

public sealed class KinDeskRules : ReactiveValidator<KinDesk>
{
    public KinDeskRules() => RuleFor(desk => desk.NextOfKin.Name).NotEmpty().WithMessage("Name the next of kin.");
}

public sealed class ServerCheckedKinDeskRules : ReactiveValidator<ServerCheckedKinDesk>
{
    public ServerCheckedKinDeskRules() => RuleFor(desk => desk.NextOfKin.Name).NotEmpty().WithMessage("Name the next of kin.").ServerOnly();
}
