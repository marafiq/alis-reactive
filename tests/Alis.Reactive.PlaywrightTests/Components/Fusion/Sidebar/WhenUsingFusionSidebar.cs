namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar;

[TestFixture]
public class WhenUsingFusionSidebar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionSidebar";

    private FusionSidebarLocator Panel => new(Page, "care-services-panel");
    private ILocator OpenMenuButton => Page.Locator("#open-menu");
    private ILocator ToggleMenuButton => Page.Locator("#toggle-menu");
    private ILocator CollapseMenuButton => Page.Locator("#collapse-menu");
    private ILocator CloseMenuButton => Page.Locator("#close-menu");
    private ILocator DashboardContent => Page.Locator("#dashboard-content");
    private ILocator MenuState => Page.Locator("#menu-state");
    private ILocator ServicesSummary => Page.Locator("#services-summary");
    private ILocator ActivityNote => Page.Locator("#activity-note");

    private async Task OpenDashboard()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(OpenMenuButton).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_dashboard_opens_with_the_care_services_menu_tucked_away()
    {
        await OpenDashboard();

        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);
        await Expect(MenuState).ToHaveTextAsync("Care-services menu is tucked away.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_menu_slides_the_care_services_panel_into_view()
    {
        await OpenDashboard();

        await OpenMenuButton.ClickAsync();

        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);
        await Expect(Panel.NavLink("Care plan")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_menu_loads_the_live_care_service_list()
    {
        await OpenDashboard();

        await OpenMenuButton.ClickAsync();

        await Expect(MenuState).ToHaveTextAsync("Care-services menu is open.", new() { Timeout = 5000 });
        await Expect(ServicesSummary).ToHaveTextAsync("3 care services available", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_menu_with_the_button_tucks_it_away()
    {
        await OpenDashboard();
        await OpenMenuButton.ClickAsync();
        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);

        await CloseMenuButton.ClickAsync();

        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);
        await Expect(MenuState).ToHaveTextAsync("Care-services menu closed automatically.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dismissing_the_menu_by_tapping_the_dashboard_records_that_you_closed_it()
    {
        await OpenDashboard();
        await OpenMenuButton.ClickAsync();
        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);

        await DashboardContent.ClickAsync();

        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);
        await Expect(MenuState).ToHaveTextAsync("You closed the care-services menu.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_menu_logs_that_the_services_are_hidden()
    {
        await OpenDashboard();
        await OpenMenuButton.ClickAsync();
        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);

        await CloseMenuButton.ClickAsync();

        await Expect(ActivityNote)
            .ToHaveTextAsync("Care-services menu closed — services hidden.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_menu_posts_that_the_panel_is_now_shut()
    {
        await OpenDashboard();
        await OpenMenuButton.ClickAsync();
        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/FusionSidebar/CloseActivity") && request.Method == "POST",
            new() { Timeout = 10000 });

        await CloseMenuButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"IsOpen\":false"),
            "the gather pipeline must carry the sidebar IsOpen() source under its declared key");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_menu_button_toggles_the_panel_open_then_collapse_toggles_it_shut()
    {
        await OpenDashboard();
        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);

        await ToggleMenuButton.ClickAsync();
        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);

        await CollapseMenuButton.ClickAsync();
        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);

        AssertNoConsoleErrors();
    }
}
