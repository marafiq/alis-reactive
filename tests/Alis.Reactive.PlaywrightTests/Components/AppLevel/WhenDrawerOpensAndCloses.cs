using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

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

    [Test]
    public async Task page_loads_with_three_open_buttons()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#btn-open-sm")).ToBeVisibleAsync();
        await Expect(Page.Locator("#btn-open-md")).ToBeVisibleAsync();
        await Expect(Page.Locator("#btn-open-lg")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_small_button_opens_drawer_with_resident_details()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-sm").ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        await Expect(DrawerTitle).ToHaveTextAsync("Resident Details", new() { Timeout = 5000 });

        // Content is loaded from a partial through HTTP GET into the drawer.
        await Expect(DrawerContent).ToContainTextAsync("This drawer was opened via the Reactive Plan", new() { Timeout = 5000 });

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--sm"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_medium_button_opens_drawer_with_care_plan_notes()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-md").ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(DrawerTitle).ToHaveTextAsync("Care Plan Notes", new() { Timeout = 5000 });

        await Expect(DrawerContent).ToContainTextAsync("Memory Care", new() { Timeout = 5000 });
        await Expect(DrawerContent).ToContainTextAsync("March 2026", new() { Timeout = 5000 });

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--md"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_large_button_opens_drawer_with_add_resident_form()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#btn-open-lg"));

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(DrawerTitle).ToHaveTextAsync("Add Resident", new() { Timeout = 5000 });

        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#drawer-submit-btn")).ToBeVisibleAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--lg"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task close_button_closes_the_drawer()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-sm").ClickAsync();
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        // The page-level close button is plan-driven, unlike the drawer header close button.
        await Page.Locator("#btn-close-drawer").ClickAsync();

        await Expect(Drawer).Not.ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task drawer_header_close_button_closes_the_drawer()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-sm").ClickAsync();
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#alis-drawer-close"));

        await Expect(Drawer).Not.ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submitting_add_resident_form_with_empty_fields_shows_errors()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-lg").ClickAsync();
        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#drawer-submit-btn").ClickAsync();

        var nameError = Page.Locator("[data-valmsg-for='Name']");
        var emailError = Page.Locator("[data-valmsg-for='Email']");
        var careLevelError = Page.Locator("[data-valmsg-for='CareLevel']");

        await Expect(nameError).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(emailError).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(careLevelError).ToContainTextAsync("required", new() { Timeout = 5000 });

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filling_and_submitting_resident_form_shows_success()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-open-lg").ClickAsync();
        await Expect(Page.Locator("#drawer-resident-form")).ToBeVisibleAsync(new() { Timeout = 5000 });

        var nameInput = Page.Locator("#drawer-resident-form input[name='Name']");
        await nameInput.FillAsync("Margaret Thompson");

        var emailInput = Page.Locator("#drawer-resident-form input[name='Email']");
        await emailInput.FillAsync("margaret@sunrisefacility.com");

        await Page.GetByText("Assisted Living", new() { Exact = true }).ClickAsync();

        // Server intentionally delays the success response.
        await ClickWhenStable(Page.Locator("#drawer-submit-btn"));

        // Drawer is fully closed only after the close transition finishes.
        await Expect(Drawer).ToHaveAttributeAsync("aria-hidden", "true", new() { Timeout = 30000 });

        AssertNoConsoleErrors();
    }
}
