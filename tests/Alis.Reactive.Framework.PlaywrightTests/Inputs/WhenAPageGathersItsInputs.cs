namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAPageGathersItsInputs : AppPageTest
{
    private ILocator Bound => Page.Locator("#bound");

    private ILocator Admit => Page.GetByRole(AriaRole.Button, new() { Name = "Admit", Exact = true });

    private async Task Fill(string name, string age, string weight, string admitted, string arrives)
    {
        await Page.GetByLabel("Name").FillAsync(name);
        await Page.GetByLabel("Age").FillAsync(age);
        await Page.GetByLabel("Weight").FillAsync(weight);
        await Page.GetByLabel("Admitted on").FillAsync(admitted);
        await Page.GetByLabel("Arrives at").FillAsync(arrives);
    }

    [Test]
    public async Task each_input_is_posted_as_the_type_of_the_member_it_binds_so_the_endpoint_binds_what_was_typed()
    {
        await Open("/Inputs/Admit");
        await Fill("Ada", "41", "70.5", "October 27, 2026", "2026-10-27 05:30 UTC");

        await Admit.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("Ada, 42 next year, weighs 70.5, admitted Tuesday 2026-10-27, arrives 05:30 UTC");
    }

    [Test]
    public async Task a_blank_input_for_a_member_that_may_be_empty_is_posted_as_nothing()
    {
        await Open("/Inputs/Admit");
        await Fill("Ada", "41", "", "2026-10-27", "2026-10-27T05:30:00Z");

        await Admit.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("Ada, 42 next year, weighs unknown, admitted Tuesday 2026-10-27, arrives 05:30 UTC");
    }

    [Test]
    public async Task a_rule_the_browser_checks_holds_the_request_and_shows_its_message_beside_the_input()
    {
        await Open("/Inputs/Admit");
        await Fill("", "41", "70.5", "2026-10-27", "2026-10-27T05:30:00Z");

        await Admit.ClickAsync();

        await Expect(Page.GetByText("Name the resident.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Give an age from 18 to 120.")).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Bound).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_number_rule_compares_the_typed_text_as_a_number()
    {
        await Open("/Inputs/Admit");
        await Fill("Ada", "9", "70.5", "2026-10-27", "2026-10-27T05:30:00Z");

        await Admit.ClickAsync();
        await Expect(Page.GetByText("Give an age from 18 to 120.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Bound).ToHaveTextAsync("");

        await Page.GetByLabel("Age").FillAsync("41");
        await Admit.ClickAsync();
        await Expect(Bound).ToHaveTextAsync("Ada, 42 next year, weighs 70.5, admitted Tuesday 2026-10-27, arrives 05:30 UTC");
    }

    [Test]
    public async Task typing_into_an_input_clears_its_message_at_once()
    {
        await Open("/Inputs/Admit");
        await Fill("", "41", "70.5", "2026-10-27", "2026-10-27T05:30:00Z");
        await Admit.ClickAsync();
        await Expect(Page.GetByText("Name the resident.")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("Ada");

        await Expect(Page.GetByText("Name the resident.")).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Bound).ToHaveTextAsync("");
    }

    [Test]
    public async Task leaving_an_input_rechecks_it_and_shows_the_message_its_new_value_earns()
    {
        await Open("/Inputs/Admit");
        await Fill("", "41", "70.5", "2026-10-27", "2026-10-27T05:30:00Z");
        await Admit.ClickAsync();
        await Expect(Page.GetByText("Give an age from 18 to 120.")).ToBeHiddenAsync();

        await Page.GetByLabel("Age").FillAsync("9");
        await Page.GetByLabel("Age").PressAsync("Tab");

        await Expect(Page.GetByText("Give an age from 18 to 120.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Bound).ToHaveTextAsync("");
    }
}
