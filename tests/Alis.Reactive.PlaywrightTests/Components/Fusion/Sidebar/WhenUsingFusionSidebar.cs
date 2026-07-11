using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar;

// Journey: a care coordinator works the Resident Care Dashboard. Care-services
// navigation lives in a slide-out panel. The coordinator opens it to load the live
// service list, jumps to a workflow, and tucks it away again — either with its Close
// button or by tapping back on the dashboard.
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

    // RENDERS — the FusionSidebar builder renders the panel onto the dashboard, tucked
    // away (e-close) at first paint with the menu controls available.
    [Test]
    public async Task the_dashboard_opens_with_the_care_services_menu_tucked_away()
    {
        await OpenDashboard();

        await Expect(Panel.ClosedRoot).ToHaveCountAsync(1);
        await Expect(MenuState).ToHaveTextAsync("Care-services menu is tucked away.");

        AssertNoConsoleErrors();
    }

    // INTERACTS — clicking Open calls Show(); the panel slides into view (e-open) and the
    // care-services links it holds become reachable.
    [Test]
    public async Task opening_the_menu_slides_the_care_services_panel_into_view()
    {
        await OpenDashboard();

        await OpenMenuButton.ClickAsync();

        await Expect(Panel.OpenRoot).ToHaveCountAsync(1);
        await Expect(Panel.NavLink("Care plan")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // The Opened event fires through the .Reactive wiring; the FusionSidebarTransitionArgs
    // payload reaches the plan, which posts to the server and shows the live service list
    // the server returns.
    [Test]
    public async Task opening_the_menu_loads_the_live_care_service_list()
    {
        await OpenDashboard();

        await OpenMenuButton.ClickAsync();

        await Expect(MenuState).ToHaveTextAsync("Care-services menu is open.", new() { Timeout = 5000 });
        await Expect(ServicesSummary).ToHaveTextAsync("3 care services available", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Closing with the Close button calls Hide() — an API close, so the Closed event reports
    // IsInteracted false and the panel tucks away (e-close).
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

    // IsInteracted distinguishes a coordinator dismissing the panel themselves (tapping the
    // dashboard fires close WITH an event -> true) from the button's API close (false). This
    // proves the true branch; closing_the_menu_with_the_button_tucks_it_away proves false.
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

    // IsOpen() read source — the Close button reads the panel state after Hide() and gathers it
    // to the server, which confirms the services are hidden. The note appears only when IsOpen()
    // yields false into the post.
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

    // GATHERS — the framework gather pipeline carries the IsOpen() source into the close POST under
    // the declared key. (Framework gather test: asserts request.PostData.)
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

        Assert.That(body, Does.Contain("\"isOpen\":false"),
            "the gather pipeline must carry the sidebar IsOpen() source under its declared key");

        AssertNoConsoleErrors();
    }

    // Toggle() flips the panel from whatever state it is in: the header menu button toggles
    // the tucked-away panel open, and the in-panel Collapse toggles the open panel shut. Both
    // call Toggle(), so the test fails whether Toggle stops opening or stops closing.
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
