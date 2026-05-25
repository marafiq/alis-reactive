using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

// --- Models for nested validation scenarios ---

public class NestedConditionModel
{
    public NestedAddress? Address { get; set; }
    public NestedContact? Contact { get; set; }
    public string? Name { get; set; }
    public string? ConfirmEmail { get; set; }
}

public class NestedAddress
{
    public string? City { get; set; }
    public string? ConfirmCity { get; set; }
}

public class NestedContact
{
    public string? Email { get; set; }
}

public class NestedConditionAddressValidator : ReactiveValidator<NestedAddress>
{
    public NestedConditionAddressValidator()
    {
        // Condition on City within the nested Address validator
        WhenFieldNotEmpty(x => x.City, () =>
        {
            RuleFor(x => x.ConfirmCity).NotEmpty().WithMessage("Confirm city when city is set");
        });
    }
}

public class NestedConditionParentValidator : AbstractValidator<NestedConditionModel>
{
    public NestedConditionParentValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address!).SetValidator(new NestedConditionAddressValidator());
    }
}

public class ParentChildModel
{
    public bool ParentFlag { get; set; }
    public ParentChildInner? Child { get; set; }
}

public class ParentChildInner
{
    public bool ChildFlag { get; set; }
    public string? Name { get; set; }
}

public class ChildConditionalValidator : ReactiveValidator<ParentChildInner>
{
    public ChildConditionalValidator()
    {
        WhenField(x => x.ChildFlag, () =>
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Child name required when child flag set");
        });
    }
}

public class ParentConditionalValidator : ReactiveValidator<ParentChildModel>
{
    public ParentConditionalValidator()
    {
        WhenField(x => x.ParentFlag, () =>
        {
            RuleFor(x => x.Child!).SetValidator(new ChildConditionalValidator());
        });
    }
}

public class AddressComparisonValidator : ReactiveValidator<NestedAddress>
{
    public AddressComparisonValidator()
    {
        RuleFor(x => x.ConfirmCity).Equal(x => x.City)
            .WithMessage("Must match city")
            .ProjectToClient(rule => rule.EqualTo(x => x.City));
    }
}

public class NestedComparisonParentValidator : AbstractValidator<NestedConditionModel>
{
    public NestedComparisonParentValidator()
    {
        RuleFor(x => x.Address!).SetValidator(new AddressComparisonValidator());
    }
}

[TestFixture]
public class WhenProjectingNestedValidationIntent
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Nested_condition_field_carries_full_model_path()
    {
        var fields = _adapter.ProjectRules(typeof(NestedConditionParentValidator), "form");

        var confirmCity = fields.FirstOrDefault(f => f.FieldName == "Address.ConfirmCity");
        Assert.That(confirmCity, Is.Not.Null, "Address.ConfirmCity field should be projected");
        Assert.That(confirmCity!.Rules, Has.Count.GreaterThan(0));

        var when = confirmCity.Rules[0].Condition();
        Assert.That(when, Is.Not.Null, "Conditional rule should have a When");

        var cmp = (FieldCompare)when!;
        Assert.That(cmp.Field, Is.EqualTo("Address.City"),
            "Condition source field must carry the full nested path, not just the leaf name");
    }

    [Test]
    public void Parent_and_child_conditions_compose_into_one_client_activation()
    {
        var fields = _adapter.ProjectRules(typeof(ParentConditionalValidator), "form");

        var childName = fields.FirstOrDefault(f => f.FieldName == "Child.Name");
        Assert.That(childName, Is.Not.Null, "Child.Name field should be projected");
        Assert.That(childName!.Rules, Has.Count.GreaterThan(0));

        var when = childName.Rules[0].Condition();
        Assert.That(when, Is.Not.Null, "Should have a condition");

        var validator = new ParentConditionalValidator();
        var resultParentFalse = validator.Validate(new ParentChildModel
        {
            ParentFlag = false,
            Child = new ParentChildInner { ChildFlag = true, Name = null }
        });
        Assert.That(resultParentFalse.IsValid, Is.True,
            "Server: ParentFlag=false should skip child rules regardless of ChildFlag");

        if (when is FieldCompare singleCmp)
        {
            Assert.Fail(
                $"Only single condition projected: Field={singleCmp.Field}, Op={singleCmp.Op}. " +
                "Parent condition was discarded. Client would show error when server skips it.");
        }

        Assert.That(when, Is.InstanceOf<FieldAll>(),
            "Should be All(parent, child) composition");
    }

    [Test]
    public void Nested_validator_explicit_peer_projection_uses_same_object_model_path()
    {
        var fields = _adapter.ProjectRules(typeof(NestedComparisonParentValidator), "form");

        var confirmCity = fields.FirstOrDefault(f => f.FieldName == "Address.ConfirmCity");
        Assert.That(confirmCity, Is.Not.Null, "Address.ConfirmCity field should be projected");
        Assert.That(confirmCity!.Rules, Has.Count.GreaterThan(0));

        var rule = confirmCity.Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("equalTo"));

        Assert.That(rule.PeerFieldName(), Is.EqualTo("Address.City"),
            "Explicit peer field projection inside a nested validator must carry the parent path.");
    }

    [Test]
    public void Include_inside_client_condition_carries_parent_activation()
    {
        var fields = _adapter.ProjectRules(typeof(IncludeConditionalValidator), "form");

        var name = fields.FirstOrDefault(f => f.FieldName == "Name");
        Assert.That(name, Is.Not.Null, "Name field should be projected");
        Assert.That(name!.Rules, Has.Count.GreaterThan(0));

        var rule = name.Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("required"));

        Assert.That(rule.Condition(), Is.Not.Null,
            "Included rules inside WhenField must carry the condition");

        var when = (FieldCompare)rule.Condition()!;
        Assert.That(when.Field, Is.EqualTo("IsEmployed"));
        Assert.That(when.Op, Is.EqualTo("truthy"));

        var validator = new IncludeConditionalValidator();
        var result = validator.Validate(new TestModel { IsEmployed = false, Name = null });
        Assert.That(result.IsValid, Is.True,
            "Server: IsEmployed=false should skip included rules");
    }
}

public class SharedSectionValidator : AbstractValidator<TestModel>
{
    public SharedSectionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name required");
    }
}

public class IncludeConditionalValidator : ReactiveValidator<TestModel>
{
    public IncludeConditionalValidator()
    {
        WhenField(x => x.IsEmployed, () =>
        {
            Include(new SharedSectionValidator());
        });
    }
}
