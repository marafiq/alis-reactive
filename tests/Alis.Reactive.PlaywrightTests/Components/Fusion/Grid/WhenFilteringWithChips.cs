namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the front desk, I want to pick care levels as chips and list only the residents in them, and to be
/// told when Memory Care residents are listed, so that I send their visitors through the secured wing's desk.
/// </summary>
/// <remarks>
/// The roster holds seven residents: three in Memory Care (Ada, Cy, Gus), two Assisted (Bo, Fay), one in
/// Skilled Nursing (Di) and one Independent (Ed). Applying the filters announces the chips the chip list
/// says are selected; the page counts them, tests their values for Memory Care and sends their texts to the
/// server, which answers with the residents in those care levels.
/// </remarks>
[TestFixture]
public class WhenFilteringWithChips : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ChipFilter";

    private ILocator Residents => Page.Locator("#care-grid .e-row");
    private ILocator ApplyFilters => Page.Locator("#apply-filters-btn");
    private ILocator CareLevelsSelected => Page.Locator("#filter-count");
    private ILocator MemoryCareVisits => Page.Locator("#memory-care-visits");

    private async Task OpenTheRoster()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Residents).ToHaveCountAsync(7, new() { Timeout = 10000 });
    }

    private async Task SelectChip(string text)
    {
        var chip = Page.Locator("#care-filters .e-chip").Filter(new() { HasText = text });
        await Expect(chip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await chip.ClickAsync();
    }

    [Test]
    public async Task the_roster_opens_with_every_resident()
    {
        await OpenTheRoster();

        await Expect(Page.Locator("#care-grid")).ToContainTextAsync("Skilled Nursing");
        await Expect(MemoryCareVisits).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_two_care_levels_lists_only_their_residents()
    {
        await OpenTheRoster();
        await SelectChip("Memory Care");
        await SelectChip("Assisted");

        await ClickWhenStable(ApplyFilters);

        await Expect(Residents).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-grid")).Not.ToContainTextAsync("Skilled Nursing");
        await Expect(Page.Locator("#care-grid")).Not.ToContainTextAsync("Independent");
        await Expect(CareLevelsSelected).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task listing_memory_care_residents_sends_their_visits_through_the_secured_wing()
    {
        await OpenTheRoster();
        await SelectChip("Memory Care");

        await ClickWhenStable(ApplyFilters);

        await Expect(MemoryCareVisits).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Residents).ToHaveCountAsync(3, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The note follows the chips applied last: an Assisted-only list holds no Memory Care resident, and a
    // test that ignored the chip's value would keep the note.
    [Test]
    public async Task an_assisted_only_list_drops_the_secured_wing_note()
    {
        await OpenTheRoster();
        await SelectChip("Memory Care");
        await ClickWhenStable(ApplyFilters);
        await Expect(MemoryCareVisits).ToBeVisibleAsync(new() { Timeout = 10000 });

        await SelectChip("Memory Care");
        await SelectChip("Assisted");
        await ClickWhenStable(ApplyFilters);

        await Expect(Residents).ToHaveCountAsync(2, new() { Timeout = 10000 });
        await Expect(CareLevelsSelected).ToHaveTextAsync("1");
        await Expect(MemoryCareVisits).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
