namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleCascadesOtherwiseThanItsValidator : AppPageTest
{
    private ILocator Saved => Page.Locator("#saved");

    private ILocator Field(string label)
        => Page.Locator("#round-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_rule_that_continues_answers_with_every_failing_check_while_its_validator_stops_the_others_at_their_first()
    {
        await Open("/Validation/MedicationRound");

        await Write("Cart", "X-12");
        await Write("Nurse", "Dr-Bell");
        await Save();
        await Expect(Field("Cart").GetByText("Carts start with C-. Three characters a cart.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("Nurse").GetByText("A nurse's badge starts with RN-.", new() { Exact = true })).ToBeVisibleAsync();

        await Write("Cart", "C-2");
        await Write("Nurse", "RN-Bea");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
