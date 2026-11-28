using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StateMoveInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.StateMoveIn)]
    public IActionResult OfAResident() => View("/Care/Admission/StateMoveIn.cshtml", new StateMoveIn("Ann Lee"));

    [HttpGet(StateMoveInEndpoints.Texas)]
    public IActionResult Texas() => PartialView("/Care/Admission/StateMoveInTexas.cshtml", new StateMoveInTexas());

    [HttpGet(StateMoveInEndpoints.Massachusetts)]
    public IActionResult Massachusetts() => PartialView("/Care/Admission/StateMoveInMassachusetts.cshtml", new StateMoveInMassachusetts());

    [HttpPost(StateMoveInEndpoints.TexasMoveIns)]
    public IActionResult MoveInToTexas([FromForm] StateMoveInTexas moveIn) => Content($"{moveIn.License}", PlainText);

    [HttpPost(StateMoveInEndpoints.MassachusettsMoveIns)]
    public IActionResult MoveInToMassachusetts([FromForm] StateMoveInMassachusetts moveIn) => Content($"{moveIn.Permit}", PlainText);
}

public sealed record StateMoveIn(string Resident);

public sealed class StateMoveInTexas
{
    public string? License { get; set; }
}

public sealed class StateMoveInMassachusetts
{
    public string? Permit { get; set; }
}

public sealed class StateMoveInTexasRules : ReactiveValidator<StateMoveInTexas>
{
    public StateMoveInTexasRules() => RuleFor(moveIn => moveIn.License).NotEmpty().WithMessage("Give the Texas license.");
}

public sealed class StateMoveInMassachusettsRules : ReactiveValidator<StateMoveInMassachusetts>
{
    public StateMoveInMassachusettsRules() => RuleFor(moveIn => moveIn.Permit).NotEmpty().WithMessage("Give the Massachusetts permit.");
}

public static class StateMoveInEndpoints
{
    public const string Texas = CarePages.Area + "admission/state-move-in/texas";

    public const string Massachusetts = CarePages.Area + "admission/state-move-in/massachusetts";

    public const string TexasMoveIns = CarePages.Area + "admission/state-move-in/texas/move-ins";

    public const string MassachusettsMoveIns = CarePages.Area + "admission/state-move-in/massachusetts/move-ins";
}

public static class StateMoveInButtons
{
    public static readonly AppEvent Texas = AppEvent.Declare();

    public static readonly AppEvent Massachusetts = AppEvent.Declare();

    public static readonly AppEvent SaveTexas = AppEvent.Declare();

    public static readonly AppEvent SaveMassachusetts = AppEvent.Declare();
}
