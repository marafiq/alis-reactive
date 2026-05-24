using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

public sealed class ExplicitClientRuleModel
{
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
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

[TestFixture]
public sealed class WhenProjectingExplicitClientRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Custom_server_rule_can_declare_regex_client_projection()
    {
        var report = _adapter.ExtractReport(typeof(ExplicitClientRuleValidator), "form");

        var code = report.ClientFields.Single(field => field.FieldName == "Code");
        var rule = code.Rules.Single();

        Assert.That(rule.Rule, Is.EqualTo("regex"));
        Assert.That(rule.Message, Is.EqualTo("Code must start with ALIS."));
        Assert.That(rule.ConstraintValue(), Is.EqualTo("^ALIS-"));
        Assert.That(report.SkippedClientRules, Is.Empty);
    }

    [Test]
    public void Custom_server_rule_can_declare_peer_field_client_projection()
    {
        var report = _adapter.ExtractReport(typeof(ExplicitClientRuleValidator), "form");

        var confirmEmail = report.ClientFields.Single(field => field.FieldName == "ConfirmEmail");
        var rule = confirmEmail.Rules.Single();

        Assert.That(rule.Rule, Is.EqualTo("equalTo"));
        Assert.That(rule.PeerFieldName(), Is.EqualTo("Email"));
        Assert.That(report.ClientFields.Select(field => field.FieldName), Does.Contain("Email"));
        Assert.That(report.SkippedClientRules, Is.Empty);
    }
}
