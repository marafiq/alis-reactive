using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

public sealed class ExplicitClientRuleModel
{
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public string? ExternalCode { get; set; }
}

public sealed class ExplicitClientRuleValidator : AbstractValidator<ExplicitClientRuleModel>
{
    public ExplicitClientRuleValidator()
    {
        RuleFor(x => x.Code)
            .Must(code => string.IsNullOrEmpty(code) || code.StartsWith("ALIS-"))
            .WithMessage("Code must start with ALIS.")
            .ProjectToClient(rule => rule.Regex("^ALIS-"));

        RuleFor(x => x.ConfirmEmail)
            .Must((model, confirmation) => string.IsNullOrEmpty(confirmation) || confirmation == model.Email)
            .ProjectToClient(rule => rule.EqualTo(x => x.Email));
    }
}

public sealed class AsyncServerRuleValidator : AbstractValidator<ExplicitClientRuleModel>
{
    public AsyncServerRuleValidator()
    {
        RuleFor(x => x.ExternalCode)
            .MustAsync((_, _) => Task.FromResult(true))
            .ProjectToClient(rule => rule.Regex("^EXT-"));
    }
}

[TestFixture]
public sealed class WhenProjectingExplicitClientRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Custom_server_rule_can_declare_regex_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(ExplicitClientRuleValidator), "form");

        var code = report.Fields.Single(field => field.FieldName == "Code");
        var rule = code.Rules.Single();

        Assert.That(rule.Rule, Is.EqualTo("regex"));
        Assert.That(rule.Message, Is.EqualTo("Code must start with ALIS."));
        Assert.That(rule.ConstraintValue(), Is.EqualTo("^ALIS-"));
        Assert.That(report.SkippedRules, Is.Empty);
    }

    [Test]
    public void Custom_server_rule_can_declare_peer_field_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(ExplicitClientRuleValidator), "form");

        var confirmEmail = report.Fields.Single(field => field.FieldName == "ConfirmEmail");
        var rule = confirmEmail.Rules.Single();

        Assert.That(rule.Rule, Is.EqualTo("equalTo"));
        Assert.That(rule.PeerFieldName(), Is.EqualTo("Email"));
        Assert.That(report.Fields.Select(field => field.FieldName), Does.Contain("Email"));
        Assert.That(report.SkippedRules, Is.Empty);
    }

    [Test]
    public void Async_rules_stay_server_side_even_when_a_client_projection_is_declared()
    {
        var report = _adapter.ProjectValidation(typeof(AsyncServerRuleValidator), "form");

        Assert.That(report.Fields, Is.Empty);
        var skipped = report.SkippedRules.Single();
        Assert.That(skipped.FieldName, Is.EqualTo("ExternalCode"));
        Assert.That(skipped.Reason, Is.EqualTo(ClientRuleProjectionSkipReason.UnsupportedValidator));
    }
}
