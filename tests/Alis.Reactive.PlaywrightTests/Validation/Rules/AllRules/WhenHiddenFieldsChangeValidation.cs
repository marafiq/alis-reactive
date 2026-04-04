using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.AllRules;

[TestFixture]
public sealed class WhenHiddenFieldsChangeValidation : PlaywrightTestBase
{
    private ValidationShowcasePage Showcase => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/AllRules"));

    [Test]
    public async Task always_visible_name_is_still_required_when_extra_fields_are_hidden()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateHiddenButton);

        await Expect(Showcase.ErrorFor("Hidden_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_hidden_form_passes_while_extra_fields_stay_hidden()
    {
        await Showcase.Open();

        await Showcase.Input("Hidden_Name").FillAsync("Edith Collins");
        await ClickWhenStable(Showcase.ValidateHiddenButton);

        await Expect(Showcase.HiddenResult).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task show_extras_checkbox_reveals_and_hides_the_extra_section()
    {
        await Showcase.Open();

        await Expect(Showcase.HiddenExtras).ToBeHiddenAsync();
        await Showcase.Input("Hidden_ShowExtras").CheckAsync();
        await Expect(Showcase.HiddenExtras).ToBeVisibleAsync(new() { Timeout = 2000 });

        await Showcase.Input("Hidden_ShowExtras").UncheckAsync();
        await Expect(Showcase.HiddenExtras).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
