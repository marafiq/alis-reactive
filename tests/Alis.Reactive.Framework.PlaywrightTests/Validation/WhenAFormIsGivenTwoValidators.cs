namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A form one request validates by one validator and another request by a different one, refused where the
/// page renders with the message the developer reads: the form and both validators as C# spells them, and
/// the two ways to write it. Validators are told apart by type, so one generic validator for two models is
/// two validators.
/// </summary>
[TestFixture]
public sealed class WhenAFormIsGivenTwoValidators : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
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
