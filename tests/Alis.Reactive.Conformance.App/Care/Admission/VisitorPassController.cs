using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitorPassController : Controller
{
    private const string PlainText = "text/plain";
    private const string Page = "/Care/Admission/VisitorPass.cshtml";

    [HttpGet(CarePages.VisitorPass)]
    public IActionResult WithNoPass() => View(Page, new VisitorPass());

    [HttpGet(CarePages.VisitorPassHoldingP7)]
    public IActionResult HoldingP7() => View(Page, new VisitorPass { Pass = "P-7" });

    [HttpPost(VisitorPassEndpoints.SignIns)]
    public IActionResult SignIn([FromForm] VisitorPass arrival) => Content($"pass {arrival.Pass}", PlainText);
}

public sealed class VisitorPass
{
    public string? Visitor { get; set; }

    public string? Pass { get; set; }
}

public sealed class VisitorPassRules : ReactiveValidator<VisitorPass>
{
    public VisitorPassRules() => RuleFor(arrival => arrival.Pass).NotEmpty().WithMessage("The desk gives every visitor a pass.");
}

public static class VisitorPassEndpoints
{
    public const string SignIns = CarePages.VisitorPass + "/sign-ins";
}
