using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DeputyCodeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DeputyCode)]
    public IActionResult OfTheDeputy() => View("/Care/Admission/DeputyCode.cshtml", new DeputyCodeCard());

    [HttpPost(DeputyCodeEndpoints.Deputies)]
    public IActionResult Save([FromBody] DeputyCodeCard card) => Content($"{card.Note}, {card.Deputy.Code}", PlainText);
}

public sealed class DeputyCodeCard
{
    public DeputyCodeHolder Deputy { get; set; } = new() { Code = "A1" };

    public string? Note { get; set; }
}

public sealed class DeputyCodeHolder
{
    [JsonIgnore]
    public string Code { get; set; } = "Z9";

    public string? Phone { get; set; }
}

public sealed class DeputyCodeRules : ReactiveValidator<DeputyCodeHolder>
{
    public DeputyCodeRules() => RuleFor(deputy => deputy.Code).Equal("A1").WithMessage("Code the deputy A1.");
}

public sealed class DeputyCodeCardRules : ReactiveValidator<DeputyCodeCard>
{
    public DeputyCodeCardRules() => RuleFor(card => card.Deputy).SetValidator(new DeputyCodeRules());
}

public static class DeputyCodeEndpoints
{
    public const string Deputies = CarePages.DeputyCode + "/deputies";
}
