namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenANullableValueIsCompared : AppPageTest
{
    [Test]
    public async Task an_intake_over_its_target_holds()
    {
        await Open("/Balances/IntakeTargets");
        await Balance("Target 1000 ml").ClickAsync();
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 500 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check against the target" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("over target");
    }

    [Test]
    public async Task an_intake_under_its_target_does_not_hold()
    {
        await Open("/Balances/IntakeTargets");
        await Balance("Target 1000 ml").ClickAsync();
        await Balance("Add 500 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check against the target" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("within target");
    }

    [Test]
    public async Task an_ordering_with_a_null_target_does_not_hold()
    {
        await Open("/Balances/IntakeTargets");
        await Balance("Add 500 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check against the target" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("within target");
    }

    [Test]
    public async Task a_number_widened_to_its_nullable_is_written_to_a_nullable_property()
    {
        await Open("/Balances/IntakeTargets");
        await Balance("Add 500 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Keep the intake as the target" }).ClickAsync();

        await Expect(Page.Locator("#balance .balance-target")).ToHaveTextAsync("target 500 ml");
    }

    [Test]
    public async Task a_text_widened_to_its_nullable_is_passed_as_a_methods_argument()
    {
        await Open("/Pumps/CaptionedBySummary");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Caption the pump with its summary" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("rate none");
    }

    private ILocator Balance(string button) => Page.Locator("#balance").GetByRole(AriaRole.Button, new() { Name = button });
}
