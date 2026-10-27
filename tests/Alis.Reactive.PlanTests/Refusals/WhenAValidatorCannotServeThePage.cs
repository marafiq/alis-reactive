using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>The rules a page carries come from a validator the app registered and wrote as rules; anything else is refused naming the fix.</summary>
[TestFixture]
public class WhenAValidatorCannotServeThePage
{
    [Test]
    public void a_page_naming_a_validator_with_no_rule_source_registered_is_refused()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render(Registered.Nothing()));

        Assert.That(refused!.Message, Does.Contain("no IBrowserRuleSource is registered"));
        Assert.That(refused.Message, Does.Contain("AddReactiveFluentValidation"));
    }

    [Test]
    public void a_page_naming_a_validator_the_app_did_not_register_is_refused()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render(Registered.Validator<UnreadableAdmissionRules>()));

        Assert.That(refused!.Message, Does.Contain($"No validator '{typeof(AdmissionRules).FullName}' is registered"));
        Assert.That(refused.Message, Does.Contain("validation.Add<TValidator>()"));
    }

    [Test]
    public void a_message_after_a_child_validator_is_refused_naming_a_check()
    {
        var refused = Assert.Throws<InvalidOperationException>(() => new MessageAfterChildRules());

        Assert.That(refused!.Message, Does.Contain("WithMessage names the check written just before it"));
        Assert.That(refused.Message, Does.Contain(".NotEmpty().WithMessage(..)"));
    }

    [Test]
    public void a_rule_on_a_computed_value_is_refused()
    {
        var refused = Assert.Throws<ArgumentException>(() => new ComputedMemberRules());

        Assert.That(refused!.Message, Does.Contain("A rule names a member of the model"));
        Assert.That(refused.Message, Does.Contain("Name.Trim()"));
    }
}
