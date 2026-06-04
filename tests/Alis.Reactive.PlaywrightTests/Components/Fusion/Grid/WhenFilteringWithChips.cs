namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// Exercises a multi-select ChipList filter whose selected chip objects drive array DSL conditions
/// and server-side resident grid filtering.
/// </summary>
[TestFixture]
public class WhenFilteringWithChips : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ChipFilter";

    private async Task NavigateAndLoad()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("all residents", new() { Timeout = 10000 });
    }

    private async Task SelectChip(string text)
    {
        var chip = Page.Locator("#care-filters .e-chip").Filter(new() { HasText = text });
        await Expect(chip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await chip.ClickAsync();
    }

    [Test]
    public async Task grid_shows_all_residents_on_load()
    {
        await NavigateAndLoad();
        await Expect(Page.Locator("#care-grid .e-row")).ToHaveCountAsync(7, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_chips_counts_the_selection_and_guards_by_member()
    {
        await NavigateAndLoad();

        await SelectChip("Memory Care");
        await SelectChip("Assisted");
        await Page.Locator("#apply-filters-btn").ClickAsync();

        await Expect(Page.Locator("#filter-count")).ToHaveTextAsync("2", new() { Timeout = 10000 });
        await Expect(Page.Locator("#memory-on")).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_chip_selection_filters_the_grid()
    {
        await NavigateAndLoad();

        await SelectChip("Memory Care");
        await SelectChip("Assisted");
        await Page.Locator("#apply-filters-btn").ClickAsync();

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("filtered", new() { Timeout = 10000 });

        await Expect(Page.Locator("#care-grid .e-row")).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-grid")).Not.ToContainTextAsync("Skilled Nursing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
