using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SignInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.SignInWithTwoNames)]
    public IActionResult WithTwoNames() => View("/Care/Admission/SignInWithTwoNames.cshtml", new SignIn());

    [HttpGet(CarePages.SignInWithANickname)]
    public IActionResult WithANickname() => View("/Care/Admission/SignInWithANickname.cshtml", new SignIn());

    [HttpPost(SignInEndpoints.SignIns)]
    public IActionResult Save([FromForm] SignIn signIn) => Content($"name {signIn.Name}", PlainText);
}

public sealed class SignIn
{
    public string? Name { get; set; }
}

public sealed class SignInGuest
{
    public string? Name { get; set; }

    public string? Nickname { get; set; }
}

public static class SignInEndpoints
{
    public const string SignIns = CarePages.Area + "admission/sign-in/sign-ins";
}

public static class SignIns
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
