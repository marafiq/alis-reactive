using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class DeclarationPayloadsController : Controller
{
    [HttpGet(BenchPages.DeclarationsRequiredPayloadText)]
    public IActionResult RequiredPayloadText() => View("/Bench/Declarations/RequiredPayloadText.cshtml", new RequiredTextBoard("Alpha", "Ada Byron"));

    [HttpGet(BenchPages.DeclarationsOptionalPayloadText)]
    public IActionResult OptionalPayloadText() => View("/Bench/Declarations/OptionalPayloadText.cshtml", new OptionalTextBoard("4", "Water please", "Fell asleep"));

    [HttpGet(BenchPages.DeclarationsNullablePayloadText)]
    public IActionResult NullablePayloadText() => View("/Bench/Declarations/NullablePayloadText.cshtml", new NullableTextBoard("Ben Ng", "Ireland", "Ada Byron"));

    [HttpGet(BenchPages.DeclarationsPayloadOuter)]
    public IActionResult PayloadOuter() => View("/Bench/Declarations/PayloadOuter.cshtml", new PayloadOuterBoard("Gamma"));
}

public sealed record RequiredTextBoard(string Text, string RaisedFor);

public sealed record OptionalTextBoard(string Room, string Note, string SilentNote);

public sealed record NullableTextBoard(string FirstResident, string FirstCountry, string SecondResident);

public sealed record PayloadOuterBoard(string OuterText);

public static class RequiredTextButtons
{
    public static readonly AppEvent ShowTheText = AppEvent.Declare();
}
