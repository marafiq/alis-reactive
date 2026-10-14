using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Menu;

/// <summary>
/// As a med tech starting a shift on the nurses' station tablet, I want the staff navigation to open the page I
/// choose and get out of the way, to tell me how many pages a section holds, and to keep Billing closed to me
/// while the business office opens it, so that I reach my work in a tap and never a part of the app my role
/// does not use.
/// </summary>
/// <remarks>
/// The navigation is a hamburger menu, closed on load and opened by its menu icon or by Open navigation.
/// Choosing a page shows its title and link, loads what the page says tonight and closes the navigation; until
/// a page is chosen the navigation stays open. Billing shows with the lock icon to everyone but the business office, and its list stays closed
/// to them. The page behind the navigation is dimmed while the navigation is open. Jordan Lee, a med tech, is
/// signed in unless the tablet was switched to Pat Kim of the business office.
/// Not exercised:
/// BeforeOpen's Top and Left: a Menu's sections and its hamburger list open with 0 for both
/// (menu-base.js:970, 1073), so on this page they carry nothing a page could show.
/// OnOpen's, OnClose's and BeforeClose's Items: each carries the items of the list opening or closing, the list
/// BeforeOpen's Items carries, which the page reads.
/// BeforeItemRender for the top list: EJ2 renders it when it creates the menu, before the page boots; the page
/// reads BeforeItemRender for a section's pages, which render when the section opens.
/// </remarks>
[TestFixture]
public class WhenUsingFusionMenu : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionMenu";
    private const string SignedInAsTheBusinessOffice = Path + "?staff=business-office";

    private static readonly Regex Dimmed = new(@"\bopacity-40\b");

    private FusionMenuLocator Navigation => new(Page, "staff-nav");
    private ILocator PageContent => Page.Locator("#page-content");
    private ILocator PageTitle => Page.Locator("#page-title");
    private ILocator PageLink => Page.Locator("#page-link");
    private ILocator PageSummary => Page.Locator("#page-summary");
    private ILocator SectionOverview => Page.Locator("#section-overview");
    private ILocator LockedNote => Page.Locator("#locked-note");
    private ILocator ExternalNote => Page.Locator("#external-note");
    private ILocator ChooseFirstNote => Page.Locator("#choose-first-note");
    private ILocator OpenNavigation => Page.Locator("#open-navigation-btn");

    private async Task NavigateAndBoot(string path = Path)
    {
        await NavigateToAndWaitForBoot(path);
        await Expect(Navigation.HamburgerIcon).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task OpenTheNavigation()
    {
        await ClickWhenStable(Navigation.HamburgerIcon);
        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task OpenResidents()
    {
        await Navigation.Tap("Residents");
        await Expect(Navigation.Item("Census")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_menu_icon_opens_the_navigation_over_a_dimmed_page()
    {
        await NavigateAndBoot();
        await Expect(Navigation.Root).ToBeHiddenAsync();

        await ClickWhenStable(Navigation.HamburgerIcon);

        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PageContent).ToHaveClassAsync(Dimmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task open_navigation_on_the_page_opens_the_navigation()
    {
        await NavigateAndBoot();
        await Expect(Navigation.Root).ToBeHiddenAsync();

        await ClickWhenStable(OpenNavigation);

        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PageContent).ToHaveClassAsync(Dimmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_page_shows_it_with_its_link_and_closes_the_navigation()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();
        await OpenResidents();

        await Navigation.Tap("Census");

        await Expect(PageTitle).ToHaveTextAsync("Census", new() { Timeout = 5000 });
        await Expect(PageLink).ToHaveTextAsync("#/residents/census");
        await Expect(PageSummary).ToHaveTextAsync("42 residents are on the census tonight; 3 are out on leave.", new() { Timeout = 10000 });
        await Expect(Navigation.Root).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(PageContent).Not.ToHaveClassAsync(Dimmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tapping_a_section_says_how_many_pages_it_holds_and_keeps_the_navigation_open()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();

        await Navigation.Tap("Medications");

        await Expect(Navigation.Item("Med pass")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PageTitle).ToHaveTextAsync("Medications");
        await Expect(SectionOverview).ToHaveTextAsync("3 pages in this section.");
        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_second_tap_folds_a_section_and_leaves_the_navigation_open()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();
        await OpenResidents();

        await Navigation.Tap("Residents");

        await Expect(Navigation.Item("Census")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync();
        await Expect(PageContent).ToHaveClassAsync(Dimmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_navigation_stays_open_until_a_page_is_chosen()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();
        await Expect(ChooseFirstNote).ToBeHiddenAsync();

        await ClickWhenStable(Navigation.HamburgerIcon);

        await Expect(ChooseFirstNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Navigation.Item("Residents")).ToBeVisibleAsync();
        await Expect(PageContent).ToHaveClassAsync(Dimmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_med_tech_cannot_open_billing_and_is_told_who_can()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();

        await Navigation.Tap("Billing");

        await Expect(LockedNote).ToHaveTextAsync(
            "Billing opens for the business office; its 2 pages stay closed to Jordan Lee.", new() { Timeout = 5000 });
        await Expect(Navigation.Item("Invoices")).ToBeHiddenAsync();
        await Expect(SectionOverview).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_business_office_opens_billing()
    {
        await NavigateAndBoot(SignedInAsTheBusinessOffice);
        await OpenTheNavigation();

        await Navigation.Tap("Billing");

        await Expect(Navigation.Item("Invoices")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SectionOverview).ToHaveTextAsync("2 pages in this section.");
        await Expect(LockedNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_medications_points_out_the_page_that_leaves_the_staff_app()
    {
        await NavigateAndBoot();
        await OpenTheNavigation();
        await Expect(ExternalNote).ToBeHiddenAsync();

        await Navigation.Tap("Medications");

        await Expect(ExternalNote).ToHaveTextAsync("Pharmacy orders opens outside the staff app.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
