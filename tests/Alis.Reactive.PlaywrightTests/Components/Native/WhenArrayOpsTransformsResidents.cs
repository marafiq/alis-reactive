namespace Alis.Reactive.PlaywrightTests.Components.Native;

[TestFixture]
public class WhenArrayOpsTransformsResidents : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ArrayOps";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task counts_total_residents_from_the_loaded_array()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-total")).ToHaveTextAsync("5", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task counts_active_residents_by_member_predicate()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-active")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Ed is active and exactly 65: >= counts him (Ada, Cy, Ed), a > in its place would say 2.
    [Test]
    public async Task counts_active_seniors_by_compound_member_predicate()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-active-seniors")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sums_ages_of_active_residents_via_chained_filter_then_sum()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-active-age-sum")).ToHaveTextAsync("216", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finds_oldest_active_resident_via_filter_orderby_find()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-oldest-active")).ToHaveTextAsync("Cy", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Each count sits on a boundary of the roster: Cy is 80, Ed is 65, Ed's charges are $800, and three of
    // five are active. A >= for >, a <= for <, a < for <=, or an == for != each changes one number.
    [Test]
    public async Task the_roster_counts_residents_on_each_side_of_its_age_and_charge_boundaries()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#res-over-80")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#res-under-65")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#res-low-charges")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#res-not-active")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    // $1,200.50 + $2,000.25 + $800 and 1.5 + 2.25 + 0.75 hours: a sum that dropped the cents or the
    // fraction of an hour (an integer sum) would say 4000 and 4.
    [Test]
    public async Task the_roster_totals_active_residents_charges_to_the_cent_and_care_hours_to_the_quarter_hour()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#res-active-charges")).ToHaveTextAsync("4000.75", new() { Timeout = 5000 });
        await Expect(Page.Locator("#res-active-care-hours")).ToHaveTextAsync("4.5");
        AssertNoConsoleErrors();
    }

    // Di is the first critical resident but not the first on the roster (Ada is): a Find that ignored its
    // predicate would name Ada.
    [Test]
    public async Task the_roster_names_the_first_critical_resident_as_needing_attention_first()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-first-critical")).ToHaveTextAsync("Di", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flags_a_critical_resident_via_any_predicate_guard()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-critical-yes")).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
