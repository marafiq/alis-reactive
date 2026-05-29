namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionChipList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ChipList";

    private async Task NavigateAndWaitForChipList()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#care-tags.e-chip-list"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task add_method_appends_new_visible_chip()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#add-chip-btn").ClickAsync();

        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("add called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-tags"))
            .ToContainTextAsync("Wound Care", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_by_index_method_selects_existing_chip()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#select-index-chip-btn").ClickAsync();

        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("select index called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-tags .e-chip").Nth(1))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_by_indexes_method_selects_multiple_existing_chips()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#select-indexes-chip-btn").ClickAsync();

        await Expect(Page.Locator("#index-chip-command-status"))
            .ToHaveTextAsync("select indexes called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags .e-chip").Nth(0))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags .e-chip").Nth(2))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_by_text_and_return_sources_read_selected_and_found_chips()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#select-text-chip-btn").ClickAsync();

        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("select text called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-chip-texts"))
            .ToContainTextAsync("Hydration", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-chip-texts"))
            .ToContainTextAsync("Wound Care", new() { Timeout = 5000 });
        await Expect(Page.Locator("#found-chip-text"))
            .ToHaveTextAsync("Hydration", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_chip_values_property_can_be_read_after_selection()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#read-selected-values-btn").ClickAsync();

        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("selected values read", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-chip-values"))
            .ToHaveTextAsync("hydration,meds", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-tags .e-chip").Nth(1))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-tags .e-chip").Nth(2))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_chip_values_can_drive_quick_filter_payload()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#care-tags .e-chip").Filter(new() { HasTextString = "Fall Risk" }).ClickAsync();
        await Page.Locator("#care-tags .e-chip").Filter(new() { HasTextString = "Medication Review" }).ClickAsync();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#apply-chip-filters-btn").ClickAsync(),
            "**/Sandbox/Components/ChipList/QuickFilter");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("fall"), $"Quick filter payload must include fall but was '{body}'");
        Assert.That(body, Does.Contain("meds"), $"Quick filter payload must include meds but was '{body}'");

        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("filters applied", new() { Timeout = 5000 });
        await Expect(Page.Locator("#quick-filter-values"))
            .ToHaveTextAsync("fall,meds", new() { Timeout = 5000 });
        await Expect(Page.Locator("#quick-filter-count"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#quick-filter-names"))
            .ToHaveTextAsync("Ava Stone,Mateo Reed,Nora Gray", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_chip_indexes_property_can_be_set_and_read()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#set-selected-indexes-btn").ClickAsync();

        await Expect(Page.Locator("#index-chip-command-status"))
            .ToHaveTextAsync("selected indexes set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-chip-indexes"))
            .ToHaveTextAsync("0,2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags .e-chip").Nth(0))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags .e-chip").Nth(2))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_and_remove_events_surface_typed_payloads()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#care-tags .e-chip").Filter(new() { HasTextString = "Hydration" }).ClickAsync();
        await Expect(Page.Locator("#chip-clicked"))
            .ToHaveTextAsync("Hydration", new() { Timeout = 5000 });

        await Page.Locator("#remove-chip-btn").ClickAsync();
        await Expect(Page.Locator("#chip-command-status"))
            .ToHaveTextAsync("remove called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chip-deleted"))
            .ToHaveTextAsync("Fall Risk", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task remove_by_indexes_method_removes_multiple_chips()
    {
        await NavigateAndWaitForChipList();

        await Page.Locator("#remove-indexes-chip-btn").ClickAsync();

        await Expect(Page.Locator("#index-chip-command-status"))
            .ToHaveTextAsync("remove indexes called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags"))
            .Not.ToContainTextAsync("Routine", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags"))
            .Not.ToContainTextAsync("Follow Up", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-tags"))
            .ToContainTextAsync("Urgent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_links_to_chiplist_sandbox()
    {
        await NavigateTo("/Sandbox/Components");
        await Expect(Page.Locator("a[href='/Sandbox/Components/ChipList/Index']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
