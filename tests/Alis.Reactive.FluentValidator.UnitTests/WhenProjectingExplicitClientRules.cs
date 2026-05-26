using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

public sealed class ExplicitClientRuleModel
{
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public string? ExternalCode { get; set; }
    public int Age { get; set; }
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

        RuleFor(x => x.Age)
            .Must(age => age is >= 18 and <= 65)
            .ProjectToClient(rule => rule.Range(18, 65));
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
        var fields = _adapter.ProjectFields(typeof(ExplicitClientRuleValidator), "form");

        var code = fields.Single(field => field.FieldName == "Code");
        var rule = code.Rules.Single();

        Assert.That(rule.Kind, Is.EqualTo(ValidationRuleKind.Regex));
        Assert.That(rule.Message, Is.EqualTo("Code must start with ALIS."));
        Assert.That(rule.ConstraintValue, Is.EqualTo("^ALIS-"));
    }

    [Test]
    public void Custom_server_rule_can_declare_peer_field_client_projection()
    {
        var fields = _adapter.ProjectFields(typeof(ExplicitClientRuleValidator), "form");

        var confirmEmail = fields.Single(field => field.FieldName == "ConfirmEmail");
        var rule = confirmEmail.Rules.Single();

        Assert.That(rule.Kind, Is.EqualTo(ValidationRuleKind.EqualTo));
        Assert.That(rule.PeerFieldName, Is.EqualTo("Email"));
        Assert.That(fields.Select(field => field.FieldName), Does.Contain("Email"));
    }

    [Test]
    public void Custom_server_rule_can_declare_range_client_projection()
    {
        var fields = _adapter.ProjectFields(typeof(ExplicitClientRuleValidator), "form");

        var age = fields.Single(field => field.FieldName == "Age");
        var rule = age.Rules.Single();
        var constraint = rule.ConstraintValue as object[];

        Assert.That(rule.Kind, Is.EqualTo(ValidationRuleKind.Range));
        Assert.That(rule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Number));
        Assert.That(constraint, Is.Not.Null);
        Assert.That(constraint!, Is.EqualTo(new object[] { 18, 65 }));
    }

    [Test]
    public void Async_rules_stay_server_side_even_when_a_client_projection_is_declared()
    {
        var fields = _adapter.ProjectFields(typeof(AsyncServerRuleValidator), "form");

        Assert.That(fields, Is.Empty);
    }
}
