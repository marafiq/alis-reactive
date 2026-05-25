using System;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

public class ResidentQuickEditModel
{
    public string? Nickname { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal MonthlyRate { get; set; }
}

public class ResidentQuickEditValidator : AbstractValidator<ResidentQuickEditModel>
{
    public ResidentQuickEditValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DateOfBirth).NotNull().LessThan(DateTime.Today);
        RuleFor(x => x.MonthlyRate).GreaterThan(0m);
    }
}

/// <summary>
/// Proves the existing FluentValidation projection path handles a validator whose fields
/// are intended for FusionInPlaceEditor cards — no adapter changes required for the
/// InPlaceEditor onboarding.
/// </summary>
[TestFixture]
public class WhenProjectingRulesForInPlaceEditorFields
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Quick_edit_validator_projects_all_three_fields()
    {
        var desc = _adapter.ProjectRules(typeof(ResidentQuickEditValidator), "resident-form");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Nickname"));
        Assert.That(fieldNames, Does.Contain("DateOfBirth"));
        Assert.That(fieldNames, Does.Contain("MonthlyRate"));
    }

    [Test]
    public void Nickname_has_required_and_maxLength_rules()
    {
        var desc = _adapter.ProjectRules(typeof(ResidentQuickEditValidator), "resident-form");

        var nickname = desc.First(f => f.FieldName == "Nickname");
        var rules = nickname.Rules.Select(r => r.Kind).ToList();
        Assert.That(rules, Does.Contain(ValidationRuleKind.Required));
        Assert.That(rules, Does.Contain(ValidationRuleKind.MaxLength));
    }

    [Test]
    public void DateOfBirth_has_required_rule()
    {
        var desc = _adapter.ProjectRules(typeof(ResidentQuickEditValidator), "resident-form");

        var dob = desc.First(f => f.FieldName == "DateOfBirth");
        var rules = dob.Rules.Select(r => r.Kind).ToList();
        Assert.That(rules, Does.Contain(ValidationRuleKind.Required));
    }

    [Test]
    public void MonthlyRate_has_gt_rule()
    {
        var desc = _adapter.ProjectRules(typeof(ResidentQuickEditValidator), "resident-form");

        var rate = desc.First(f => f.FieldName == "MonthlyRate");
        var rules = rate.Rules.Select(r => r.Kind).ToList();
        Assert.That(rules, Does.Contain(ValidationRuleKind.GreaterThan));
    }
}
