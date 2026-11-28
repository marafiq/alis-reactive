using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitorSignInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.VisitorSignIn)]
    public IActionResult AtTheDoor() => View("/Care/Admission/VisitorSignIn.cshtml", new VisitorSignIn());

    [HttpPost(VisitorSignInEndpoints.SignIns)]
    public IActionResult SignIn([FromForm] VisitorSignIn signIn)
    {
        var validation = this.Validate<VisitorSignIn, VisitorSignInRules>(signIn);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content($"{signIn.Visitor}", PlainText);
    }
}

public sealed class VisitorSignIn
{
    public string? Visitor { get; set; }

    public string? Relation { get; set; }
}

public sealed class VisitorSignInRules : ReactiveValidator<VisitorSignIn>
{
    public VisitorSignInRules()
        => WhenOnServer(visit => visit.Relation != null && visit.Relation.Normalize() == "Son",
            () => RuleFor(visit => visit.Visitor).NotEmpty().WithMessage("Name the visitor."));
}

public static class VisitorSignInEndpoints
{
    public const string SignIns = CarePages.VisitorSignIn + "/sign-ins";
}
