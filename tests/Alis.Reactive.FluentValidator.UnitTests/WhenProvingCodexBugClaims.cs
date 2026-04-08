using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

// --- Models for nested validator scenarios ---

public class NestedConditionModel
{
    public NestedAddress? Address { get; set; }
    public string? Name { get; set; }
}

public class NestedAddress
{
    public string? City { get; set; }
    public string? ConfirmCity { get; set; }
}

// --- Claim 1: Nested condition sources lose full path ---

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

// --- Claim 2: Parent + child conditions not composed ---

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

// --- Claim 3: Nested peer-field comparisons lose full path ---

public class AddressComparisonValidator : ReactiveValidator<NestedAddress>
{
    public AddressComparisonValidator()
    {
        RuleFor(x => x.ConfirmCity).Equal(x => x.City)
            .WithMessage("Must match city");
    }
}

public class NestedComparisonParentValidator : AbstractValidator<NestedConditionModel>
{
    public NestedComparisonParentValidator()
    {
        RuleFor(x => x.Address!).SetValidator(new AddressComparisonValidator());
    }
}

// --- Tests ---

[TestFixture]
public class WhenProvingCodexBugClaims
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    /// <summary>
    /// Claim 1: When a nested validator uses WhenField(x => x.City),
    /// the extracted condition field should be "Address.City" (with prefix),
    /// not just "City".
    /// </summary>
    [Test]
    public void Claim1_nested_condition_field_should_carry_full_path()
    {
        var fields = _adapter.ExtractRules(typeof(NestedConditionParentValidator), "form");

        var confirmCity = fields.FirstOrDefault(f => f.FieldName == "Address.ConfirmCity");
        Assert.That(confirmCity, Is.Not.Null, "Address.ConfirmCity field should be extracted");
        Assert.That(confirmCity!.Rules, Has.Count.GreaterThan(0));

        var when = confirmCity.Rules[0].When;
        Assert.That(when, Is.Not.Null, "Conditional rule should have a When");

        var cmp = (FieldCompare)when!;
        // BUG CLAIM: Field will be "City" instead of "Address.City"
        Assert.That(cmp.Field, Is.EqualTo("Address.City"),
            "Condition source field must carry the full nested path, not just the leaf name");
    }

    /// <summary>
    /// Claim 2: When parent has WhenField(ParentFlag) and child has WhenField(ChildFlag),
    /// the extracted condition should be All(ParentFlag=truthy, Child.ChildFlag=truthy),
    /// not just Child.ChildFlag=truthy.
    ///
    /// Server behavior: ParentFlag=false → child rules skipped regardless of ChildFlag.
    /// Client must match: both conditions must be present.
    /// </summary>
    [Test]
    public void Claim2_parent_and_child_conditions_should_compose()
    {
        var fields = _adapter.ExtractRules(typeof(ParentConditionalValidator), "form");

        var childName = fields.FirstOrDefault(f => f.FieldName == "Child.Name");
        Assert.That(childName, Is.Not.Null, "Child.Name field should be extracted");
        Assert.That(childName!.Rules, Has.Count.GreaterThan(0));

        var when = childName.Rules[0].When;
        Assert.That(when, Is.Not.Null, "Should have a condition");

        // BUG CLAIM: Only child condition present, parent discarded.
        // If this is a bug, `when` will be FieldCompare("Child.ChildFlag", "truthy")
        // instead of FieldAll([FieldCompare("ParentFlag", "truthy"), FieldCompare("Child.ChildFlag", "truthy")])

        // Verify server behavior first: ParentFlag=false should skip child rules
        var validator = new ParentConditionalValidator();
        var resultParentFalse = validator.Validate(new ParentChildModel
        {
            ParentFlag = false,
            Child = new ParentChildInner { ChildFlag = true, Name = null }
        });
        Assert.That(resultParentFalse.IsValid, Is.True,
            "Server: ParentFlag=false should skip child rules regardless of ChildFlag");

        // Now check client extraction matches server
        // If only child condition is extracted, client would show error when ParentFlag=false + ChildFlag=true
        // That's a server/client mismatch
        if (when is FieldCompare singleCmp)
        {
            Assert.Fail(
                $"BUG CONFIRMED: Only single condition extracted: Field={singleCmp.Field}, Op={singleCmp.Op}. " +
                "Parent condition was discarded. Client would show error when server skips it.");
        }

        // Expected: composed condition with both parent and child
        Assert.That(when, Is.InstanceOf<FieldAll>(),
            "Should be All(parent, child) composition");
    }

    /// <summary>
    /// Claim 3: When a nested validator has Equal(x => x.City),
    /// the extracted peer field should be "Address.City" (with prefix),
    /// not just "City".
    /// </summary>
    [Test]
    public void Claim3_nested_peer_field_should_carry_full_path()
    {
        var fields = _adapter.ExtractRules(typeof(NestedComparisonParentValidator), "form");

        var confirmCity = fields.FirstOrDefault(f => f.FieldName == "Address.ConfirmCity");
        Assert.That(confirmCity, Is.Not.Null, "Address.ConfirmCity field should be extracted");
        Assert.That(confirmCity!.Rules, Has.Count.GreaterThan(0));

        var rule = confirmCity.Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("equalTo"));

        // BUG CLAIM: Field will be "City" instead of "Address.City"
        Assert.That(rule.Field, Is.EqualTo("Address.City"),
            "Peer field in cross-property comparison must carry the full nested path");
    }
}
