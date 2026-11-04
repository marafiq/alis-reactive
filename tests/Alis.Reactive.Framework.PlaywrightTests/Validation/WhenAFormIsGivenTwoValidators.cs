namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAFormIsGivenTwoValidators : AppPageTest
{
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Validation/HandoverRefused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_form_validated_by_two_validators_is_refused_naming_the_form_and_both()
    {
        await Render("DraftAndHandoverRules");

        await Expect(Message(
            "Form 'handover-form' is validated by HandoverDraftRules and by HandoverRules, but a form takes one validator. " +
            "Give each validator a form of its own, or write one validator whose rules differ by When conditions.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task one_generic_validator_for_two_models_is_two_validators_and_is_refused()
    {
        await Render("OneRulesForTwoNotes");

        await Expect(Message(
            "Form 'handover-form' is validated by NoteRules<HandoverNote> and by NoteRules<IncidentNote>, but a form takes one validator. " +
            "Give each validator a form of its own, or write one validator whose rules differ by When conditions.")).ToBeVisibleAsync();
    }
}
