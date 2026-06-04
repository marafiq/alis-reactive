namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises closed array DSL operations over an HTTP-loaded resident roster.
/// </summary>
/// <remarks>
/// Covers <c>Count</c> and <c>Where(...).Sum(...)</c> without plugin or hand-written JavaScript.
/// </remarks>
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

    [Test]
    public async Task counts_active_seniors_by_compound_member_predicate()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-active-seniors")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sums_ages_of_active_residents_via_chained_filter_then_sum()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-active-age-sum")).ToHaveTextAsync("206", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finds_oldest_active_resident_via_filter_orderby_find()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#res-oldest-active")).ToHaveTextAsync("Cy", new() { Timeout = 5000 });
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
