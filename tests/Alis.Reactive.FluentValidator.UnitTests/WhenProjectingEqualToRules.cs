using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingEqualToRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void FluentValidation_peer_comparison_requires_explicit_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(EqualToValidator), "testForm");

        Assert.That(report.Fields.Select(field => field.FieldName), Does.Not.Contain("ConfirmEmail"));
        var skipped = report.SkippedRules.Single();
        Assert.That(skipped.FieldName, Is.EqualTo("ConfirmEmail"));
        Assert.That(skipped.Reason, Is.EqualTo(ClientRuleProjectionSkipReason.PeerComparisonRequiresExplicitProjection));
    }

    [Test]
    public void Explicit_peer_comparison_projection_uses_custom_message()
    {
        var desc = _adapter.ProjectRules(typeof(EqualToWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var field = desc.First(f => f.FieldName == "ConfirmEmail");
        var equalRule = field.Rules.First(r => r.Kind == ValidationRuleKind.EqualTo);
        Assert.That(equalRule.Message, Is.EqualTo("Emails must match."));
        Assert.That(equalRule.PeerFieldName(), Is.EqualTo("Email"));
    }

    [Test]
    public void Equal_to_literal_null_projects_explicit_null_constraint()
    {
        var desc = _adapter.ProjectRules(typeof(LiteralNullComparisonValidator), "testForm");

        var field = desc.First(f => f.FieldName == "MiddleName");
        var equalRule = field.Rules.First(r => r.Kind == ValidationRuleKind.EqualTo);

        Assert.That(equalRule.HasConstraintOperand(), Is.True);
        Assert.That(equalRule.ConstraintValue(), Is.Null);
        Assert.That(equalRule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.None));
    }

    [Test]
    public void Not_equal_literal_null_projects_explicit_null_constraint()
    {
        var desc = _adapter.ProjectRules(typeof(LiteralNullComparisonValidator), "testForm");

        var field = desc.First(f => f.FieldName == "JobTitle");
        var notEqualRule = field.Rules.First(r => r.Kind == ValidationRuleKind.NotEqual);

        Assert.That(notEqualRule.HasConstraintOperand(), Is.True);
        Assert.That(notEqualRule.ConstraintValue(), Is.Null);
        Assert.That(notEqualRule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.None));
    }
}
