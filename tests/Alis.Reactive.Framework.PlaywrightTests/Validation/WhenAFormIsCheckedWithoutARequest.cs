namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAFormIsCheckedWithoutARequest : AppPageTest
{
    private const string NoResident = "Name the resident who fell.";
    private const string NoLocation = "Say where the fall happened.";
    private const string NoInjury = "Describe the injury for the nurse.";

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Field(string container, string label)
        => Page.Locator($"#{container} > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    [Test]
    public async Task a_check_shows_the_message_of_each_rule_the_form_breaks_and_none_for_a_rule_it_keeps()
    {
        await Open("/Validation/FallReport");
        await Write("Resident", "Ada");

        await Button("Check the report").ClickAsync();

        await Expect(Field("nurse-section", "Where it happened").GetByText(NoLocation)).ToBeVisibleAsync();
        await Expect(Page.GetByText(NoResident)).ToBeHiddenAsync();
    }

    [Test]
    public async Task the_steps_after_a_check_run_when_the_form_breaks_a_rule()
    {
        await Open("/Validation/FallReport");

        await Button("Check the report").ClickAsync();

        await Expect(Field("fall-form", "Resident").GetByText(NoResident)).ToBeVisibleAsync();
        await Expect(Page.Locator("#fall-status")).ToHaveTextAsync("checked at the desk");
    }

    [Test]
    public async Task the_steps_after_a_check_run_when_the_form_breaks_no_rule()
    {
        await Open("/Validation/FallReport");
        await Write("Resident", "Ada");
        await Write("Where it happened", "the dining room");

        await Button("Check the report").ClickAsync();

        await Expect(Page.Locator("#fall-status")).ToHaveTextAsync("checked at the desk");
        await Expect(Page.GetByText(NoLocation)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_check_by_a_rule_set_shows_the_sets_messages_and_none_for_the_rules_outside_it()
    {
        await Open("/Validation/FallReport");

        await Button("Check it for the nurse").ClickAsync();

        await Expect(Field("nurse-section", "Injury").GetByText(NoInjury)).ToBeVisibleAsync();
        await Expect(Page.GetByText(NoLocation)).ToBeHiddenAsync();
    }
}
