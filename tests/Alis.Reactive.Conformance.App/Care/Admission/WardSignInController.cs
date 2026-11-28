using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardSignInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WardSignIn)]
    public IActionResult AtTheWard() => View("/Care/Admission/WardSignIn.cshtml", new WardSignIn());

    [HttpPost(WardSignInEndpoints.SignIns)]
    public IActionResult SignIn([FromForm] WardSignIn signIn)
    {
        var validation = this.Validate<WardSignIn, WardSignInRules>(signIn);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content($"{signIn.Ward}", PlainText);
    }
}

public sealed class WardSignIn
{
    public string Name { get; set; } = "";

    public string? Ward { get; set; }
}

public sealed class WardSignInRules : ReactiveValidator<WardSignIn>
{
    public WardSignInRules() => RuleFor(signIn => signIn.Ward).NotEmpty();
}

public static class WardSignInEndpoints
{
    public const string SignIns = CarePages.WardSignIn + "/sign-ins";
}
