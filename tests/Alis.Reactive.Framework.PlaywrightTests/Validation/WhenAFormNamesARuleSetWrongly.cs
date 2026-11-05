namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAFormNamesARuleSetWrongly : AppPageTest
{
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_form_naming_a_set_the_validator_does_not_declare_is_refused_naming_the_declared_sets()
    {
        await Render("/Validation/Discharge?stage=Drafts");

        await Expect(Message("DischargeRules declares no rule set 'Drafts'. It declares: Draft, SignOff.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_form_validated_under_two_sets_is_refused_naming_the_form_the_validator_and_both_sets()
    {
        await Render("/Validation/DischargeRefused");

        await Expect(Message(
            "Form 'discharge-form' is validated by DischargeRules under rule set 'Draft' and under rule set 'SignOff', " +
            "but a form takes one validator and one rule set. Give each a form of its own.")).ToBeVisibleAsync();
    }
}
