namespace Alis.Reactive.Poc.PlaywrightTests.Promises;

[TestFixture]
public sealed class WhenAResidentIsLookedUp : AppPageTest
{
    [Test]
    public async Task the_steps_after_a_payloads_lookup_wait_until_its_promise_resolves()
    {
        await Open("/Admissions/Finder");
        await Page.GetByLabel("Find a resident").FillAsync("Helen Park");
        await Page.GetByLabel("Find a resident").PressAsync("Tab");
        await Expect(Page.Locator("#finder-outcome")).ToHaveTextAsync("Looking up");
        await Expect(Page.Locator("#match-count")).ToHaveTextAsync("-");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the lookup" }).ClickAsync();

        await Expect(Page.Locator("#finder-outcome")).ToHaveTextAsync("Found");
    }

    [Test]
    public async Task a_member_two_levels_into_the_data_a_lookup_resolves_to_is_shown()
    {
        await Open("/Admissions/Finder");
        await Page.GetByLabel("Find a resident").FillAsync("Helen Park");
        await Page.GetByLabel("Find a resident").PressAsync("Tab");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the lookup" }).ClickAsync();

        await Expect(Page.Locator("#match-wing")).ToHaveTextAsync("Rose");
    }

    [Test]
    public async Task a_member_past_one_the_resolved_data_holds_as_null_shows_its_fallback()
    {
        await Open("/Admissions/Finder");
        await Page.GetByLabel("Find a resident").FillAsync("Sam Park");
        await Page.GetByLabel("Find a resident").PressAsync("Tab");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the lookup" }).ClickAsync();

        await Expect(Page.Locator("#match-wing")).ToHaveTextAsync("no wing on record");
    }

    [Test]
    public async Task a_lookup_that_finds_no_one_takes_the_arm_its_count_holds()
    {
        await Open("/Admissions/Finder");
        await Page.GetByLabel("Find a resident").FillAsync("Ada Byron");
        await Page.GetByLabel("Find a resident").PressAsync("Tab");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the lookup" }).ClickAsync();

        await Expect(Page.Locator("#finder-outcome")).ToHaveTextAsync("Not on the register");
    }

    [Test]
    public async Task a_failed_lookup_stops_its_pipeline_before_its_next_step()
    {
        await Open("/Admissions/Finder");
        await Page.GetByLabel("Find a resident").FillAsync("Helen Park");
        await Page.GetByLabel("Find a resident").PressAsync("Tab");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fail the lookup" }).ClickAsync();

        await Expect(Page.Locator(".finder-status")).ToHaveTextAsync("Lookup: failed");
        await Expect(Page.Locator("#finder-outcome")).ToHaveTextAsync("Looking up");
    }
}
