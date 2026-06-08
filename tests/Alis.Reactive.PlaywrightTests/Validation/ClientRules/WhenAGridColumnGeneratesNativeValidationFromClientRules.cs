using Alis.Reactive.Fusion.Components;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.PlaywrightTests.Validation.ClientRules;

/// <summary>
/// Grid column validation reuses <c>ReactiveValidator</c> client metadata to build
/// EJ2 <c>column.validationRules</c>. Single-field rules with an EJ2 equivalent keep
/// their FluentValidation messages; conditional, cross-field, and unsupported rules
/// stay server-authoritative.
/// </summary>
[TestFixture]
public class WhenAGridColumnGeneratesNativeValidationFromClientRules
{
    private static IClientValidationRuleSource RuleSource()
    {
        var services = new ServiceCollection();
        services.AddReactiveFluentValidation(rules =>
        {
            rules.Add<BuiltInClientRulesValidator>();
            rules.Add<ReactiveAssessmentValidator>();
        });
        return services.BuildServiceProvider().GetRequiredService<IClientValidationRuleSource>();
    }

    private static FusionGridFieldValidation<TRow> ValidationFor<TValidator, TRow>()
        where TValidator : class
        where TRow : class =>
        FusionGridValidation.From<TValidator, TRow>(RuleSource());

    [Test]
    public void string_rules_map_directly_with_their_fluentvalidation_messages()
    {
        var validation = ValidationFor<BuiltInClientRulesValidator, BuiltInClientRulesModel>();

        var rules = (IDictionary<string, object>)validation.Field(m => m.Name)!;

        Assert.That(rules.Keys, Is.EquivalentTo(new[] { "required", "minLength", "maxLength", "regex" }));

        var required = (object[])rules["required"];
        Assert.That(required[0], Is.EqualTo(true));
        Assert.That(required[1], Is.EqualTo("'Name' is required."));

        var minLength = (object[])rules["minLength"];
        Assert.That(minLength[0], Is.EqualTo(2));
        Assert.That(minLength[1], Is.EqualTo("'Name' must be at least 2 characters."));

        var regex = (object[])rules["regex"];
        Assert.That(regex[0], Is.EqualTo("^[A-Z]+$"));
    }

    [Test]
    public void numeric_rules_map_only_where_ej2_has_an_equivalent()
    {
        var validation = ValidationFor<BuiltInClientRulesValidator, BuiltInClientRulesModel>();

        var rules = (IDictionary<string, object>)validation.Field(m => m.Score)!;

        // Range -> range, GreaterThanOrEqualTo -> min, LessThanOrEqualTo -> max.
        // ExclusiveRange, GreaterThan, LessThan, EqualTo, NotEqual: no EJ2 built-in.
        Assert.That(rules.Keys, Is.EquivalentTo(new[] { "range", "min", "max" }));

        var range = (object[])rules["range"];
        Assert.That((object[])range[0], Is.EqualTo(new object[] { 1, 5 }));
        Assert.That(range[1], Is.EqualTo("'Score' must be between 1 and 5."));
        Assert.That(((object[])rules["min"])[0], Is.EqualTo(2));
        Assert.That(((object[])rules["max"])[0], Is.EqualTo(8));
    }

    [Test]
    public void email_maps_and_rules_without_an_ej2_equivalent_produce_no_client_validation()
    {
        var validation = ValidationFor<BuiltInClientRulesValidator, BuiltInClientRulesModel>();

        var email = (IDictionary<string, object>)validation.Field(m => m.Email)!;
        Assert.That(email.Keys, Is.EquivalentTo(new[] { "email" }));

        // CreditCard and Empty have no EJ2 FormValidator equivalent: server-authoritative.
        Assert.That(validation.Field(m => m.Card), Is.Null);
        Assert.That(validation.Field(m => m.EmptyCode), Is.Null);
    }

    [Test]
    public void conditional_when_field_rules_are_never_emitted_as_always_on_column_rules()
    {
        var validation = ValidationFor<ReactiveAssessmentValidator, AssessmentModel>();

        // Score's only rule is Required under WhenField(IsVeteran): conditional, so skipped.
        Assert.That(validation.Field(m => m.Score), Is.Null);
    }
}
