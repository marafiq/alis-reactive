namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

[TestFixture]
public sealed class WhenEachCardStartsAPlanOfItsOwn : AppPageTest
{
    [Test]
    public async Task each_cards_save_sends_only_the_inputs_inside_its_card()
    {
        await Open("/Plans/Contacts");
        await Page.GetByLabel("The next of kin's name", new() { Exact = true }).FillAsync("Tom");
        await Page.GetByLabel("The alternate's name", new() { Exact = true }).FillAsync("Ann");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the next of kin", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#nextofkin-saved")).ToHaveTextAsync("next of kin Tom, alternate none");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the alternate", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#alternate-saved")).ToHaveTextAsync("next of kin none, alternate Ann");
    }

    [Test]
    public async Task a_cards_plan_that_validates_shows_a_message_the_server_sends_for_a_member_it_does_not_render()
    {
        await Open("/Plans/Keyholders");
        await Page.GetByLabel("The keyholder's name", new() { Exact = true }).FillAsync("Tom");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the keyholder", Exact = true }).ClickAsync();

        await Expect(Page.GetByText("Say how the keyholder is related.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_cards_plan_that_neither_validates_nor_shows_a_forms_errors_renders_no_validation_summary()
    {
        await Open("/Plans/Contacts");

        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToHaveCountAsync(0);
    }
}
