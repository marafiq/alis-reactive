using System;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareRotaController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.CareRota)]
    public IActionResult OfAResident() => View("/Care/Record/CareRota.cshtml", new CareRota());

    [HttpPost(CareRotaEndpoints.CareSaves)]
    public IActionResult SaveCare([FromForm] CareRota plan)
        => Content($"{plan.Name}, {plan.Care}", PlainText);

    [HttpPost(CareRotaEndpoints.RotaSaves)]
    public IActionResult SaveRota([FromForm] CareRota plan)
        => Content($"{plan.Name}, {plan.Rota}", PlainText);

    [HttpGet(CarePages.CareRotaFromEarly)]
    public IActionResult FromEarly() => View("/Care/Record/EarlyCareRota.cshtml", new EarlyCareRota());

    [HttpPost(EarlyCareRotaEndpoints.CareSaves)]
    public IActionResult SaveEarlyCare([FromForm] EarlyCareRota plan)
        => Content($"{plan.Name}, {plan.Care}", PlainText);

    [HttpPost(EarlyCareRotaEndpoints.RotaSaves)]
    public IActionResult SaveEarlyRota([FromForm] EarlyCareRota plan)
        => Content($"{plan.Name}, {plan.Rota}", PlainText);
}

[Flags]
public enum CareDays
{
    Mon = 1,
    Tue = 2,
    Wed = 4,
}

[Flags]
public enum RotaDays
{
    Early = 1,
    Late = 2,
}

public sealed class CareRota
{
    public string? Name { get; set; }

    public CareDays Care { get; set; } = CareDays.Mon | CareDays.Tue;

    public RotaDays Rota { get; set; }
}

public sealed class EarlyCareRota
{
    public string? Name { get; set; }

    public CareDays Care { get; set; } = CareDays.Mon | CareDays.Tue;

    public RotaDays Rota { get; set; } = RotaDays.Early;
}

public sealed class CareRotaRules : ReactiveValidator<CareRota>
{
    public const string Care = nameof(Care);

    public const string Rota = nameof(Rota);

    public CareRotaRules()
    {
        RuleSet(Care, () => RuleFor(rota => rota.Care).IsInEnum());
        RuleSet(Rota, () => RuleFor(rota => rota.Rota).IsInEnum());
    }
}

public sealed class EarlyCareRotaRules : ReactiveValidator<EarlyCareRota>
{
    public const string Care = nameof(Care);

    public const string Rota = nameof(Rota);

    public EarlyCareRotaRules()
    {
        RuleSet(Care, () => RuleFor(rota => rota.Care).IsInEnum());
        RuleSet(Rota, () => RuleFor(rota => rota.Rota).IsInEnum());
    }
}

public static class CareRotaEndpoints
{
    public const string CareSaves = CarePages.CareRota + "/care-saves";

    public const string RotaSaves = CarePages.CareRota + "/rota-saves";
}

public static class EarlyCareRotaEndpoints
{
    public const string CareSaves = CarePages.CareRotaFromEarly + "/care-saves";

    public const string RotaSaves = CarePages.CareRotaFromEarly + "/rota-saves";
}
