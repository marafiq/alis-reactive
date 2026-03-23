using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a facility administrator using the slide-out drawer
/// I want to open drawers of different sizes with different content
/// So that I can view resident details, care plan notes, or add a new resident
///
/// Page under test: /Sandbox/Components/Drawer
/// </summary>
[TestFixture]
public class WhenDrawerOpensAndCloses : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator DrawerTitle => Page.Locator("#alis-drawer-title");
    private ILocator DrawerContent => Page.Locator("#alis-drawer-content");

    // ── Page structure ──

    [Test]
    public async Task page_loads_with_three_open_buttons()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#btn-open-sm")).ToBeVisibleAsync();
        await Expect(Page.Locator("#btn-open-md")).ToBeVisibleAsync();
        await Expect(Page.Locator("#btn-open-lg")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Small drawer: Resident Details ──

    [Test]
    public async Task clicking_small_button_opens_drawer_with_resident_details()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-sm").ClickAsync();

        // Drawer becomes visible (alis-drawer--visible class controls CSS visibility)
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        // Title set by plan: "Resident Details"
        await Expect(DrawerTitle).ToHaveTextAsync("Resident Details", new() { Timeout = 5000 });

        // Content loaded from partial (HTTP GET → Into)
        await Expect(DrawerContent).ToContainTextAsync("This drawer was opened via the reactive plan", new() { Timeout = 5000 });

        // Size class applied
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--sm"));

        AssertNoConsoleErrors();
    }

    // ── Medium drawer: Care Plan Notes ──

    [Test]
    public async Task clicking_medium_button_opens_drawer_with_care_plan_notes()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-md").ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(DrawerTitle).ToHaveTextAsync("Care Plan Notes", new() { Timeout = 5000 });

        // Care plan partial shows key-value pairs
        await Expect(DrawerContent).ToContainTextAsync("Memory Care", new() { Timeout = 5000 });
        await Expect(DrawerContent).ToContainTextAsync("March 2026", new() { Timeout = 5000 });

        // Size class applied
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--md"));

        AssertNoConsoleErrors();
    }

    // ── Large drawer: Add Resident Form ──

    [Test]
    public async Task clicking_large_button_opens_drawer_with_add_resident_form()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-lg").ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(DrawerTitle).ToHaveTextAsync("Add Resident", new() { Timeout = 5000 });

        // Form loaded — verify key form elements are present
        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#drawer-submit-btn")).ToBeVisibleAsync();

        // Size class applied
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--lg"));

        AssertNoConsoleErrors();
    }

    // ── Closing the drawer ──

    [Test]
    public async Task close_button_closes_the_drawer()
    {
        await NavigateAndBoot();

        // Open drawer first
        await Page.Locator("#btn-open-sm").ClickAsync();
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        // Close via the page-level close button (plan-driven)
        await Page.Locator("#btn-close-drawer").ClickAsync();

        // Drawer loses the visible class
        await Expect(Drawer).Not.ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // TODO: drawer_x_button_closes_the_drawer — needs investigation.
    // The #alis-drawer-close button exists (rendered by NativeDrawerExtensions)
    // but Playwright click times out. May need to wait for drawer CSS transition
    // to complete before the X button becomes clickable.

    // ── Form validation: empty submit ──

    [Test]
    public async Task submitting_add_resident_form_with_empty_fields_shows_errors()
    {
        await NavigateAndBoot();

        // Open large drawer with add resident form
        await Page.Locator("#btn-open-lg").ClickAsync();
        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Submit with all fields empty
        await Page.Locator("#drawer-submit-btn").ClickAsync();

        // Validation errors for required fields: Name, Email, CareLevel
        var nameError = Page.Locator("[data-valmsg-for='Name']");
        var emailError = Page.Locator("[data-valmsg-for='Email']");
        var careLevelError = Page.Locator("[data-valmsg-for='CareLevel']");

        await Expect(nameError).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(emailError).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(careLevelError).ToContainTextAsync("required", new() { Timeout = 5000 });

        // Drawer stays open while errors are showing
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"));

        AssertNoConsoleErrors();
    }

    // ── Form: fill and submit successfully ──

    [Test]
    public async Task filling_and_submitting_resident_form_shows_success()
    {
        await NavigateAndBoot();

        // Open large drawer with add resident form
        await Page.Locator("#btn-open-lg").ClickAsync();
        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Fill required fields
        var nameInput = Page.Locator("#drawer-resident-form input[name='Name']");
        await nameInput.FillAsync("Margaret Thompson");

        var emailInput = Page.Locator("#drawer-resident-form input[name='Email']");
        await emailInput.FillAsync("margaret@sunrisefacility.com");

        // Select care level via radio button label
        await Page.GetByText("Assisted Living", new() { Exact = true }).ClickAsync();

        // Submit — server has a 5s delay, then returns success
        await Page.Locator("#drawer-submit-btn").ClickAsync();

        // Drawer closes on success (plan calls NativeDrawer.Close after toast)
        await Expect(Drawer).Not.ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 15000 });

        AssertNoConsoleErrors();
    }
}
