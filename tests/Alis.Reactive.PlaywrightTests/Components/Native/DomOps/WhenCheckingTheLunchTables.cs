namespace Alis.Reactive.PlaywrightTests.Components.Native.DomOps;

[TestFixture]
public class WhenCheckingTheLunchTables : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DomOps";

    private ILocator Figure(int table, string figure) => Page.Locator($"#table-{table}-{figure}");
    private ILocator MemoryCareNote(int table) => Page.Locator($"#table-{table}-memory-care");
    private ILocator FallRiskBadge(int table) => Page.Locator($"#table-{table}-fall-risk");
    private ILocator FlagAFallRisk(int table) => Page.Locator($"#table-{table}-flag-fall-risk-btn");

    private async Task OpenTheSeating()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Figure(4, "seated")).ToHaveTextAsync("4", new() { Timeout = 10000 });
    }

    [Test]
    public async Task each_table_card_counts_the_residents_seated_there()
    {
        await OpenTheSeating();

        await Expect(Figure(5, "seated")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task each_table_card_counts_the_residents_who_need_close_supervision()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "close-supervision")).ToHaveTextAsync("2");
        await Expect(Figure(5, "close-supervision")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task every_diet_that_names_sodium_is_a_sodium_restricted_plate()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "sodium-plates")).ToHaveTextAsync("3");
        await Expect(Figure(5, "sodium-plates")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task only_low_and_reduced_sodium_diets_are_plated_from_the_low_sodium_line()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "low-sodium-line")).ToHaveTextAsync("2");
        await Expect(Figure(5, "low-sodium-line")).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task each_table_card_counts_its_flags()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "flag-count")).ToHaveTextAsync("4");
        await Expect(Figure(5, "flag-count")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task only_the_flags_that_start_risk_count_as_safety_risks()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "risk-count")).ToHaveTextAsync("2");
        await Expect(Figure(5, "risk-count")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_memory_care_table_asks_for_a_staff_member_to_sit_with_it_and_other_tables_do_not()
    {
        await OpenTheSeating();

        await Expect(MemoryCareNote(4)).ToBeVisibleAsync();
        await Expect(MemoryCareNote(5)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_a_fall_risk_adds_it_to_that_tables_flags_and_safety_risks()
    {
        await OpenTheSeating();
        await Expect(FallRiskBadge(4)).ToBeHiddenAsync();

        await ClickWhenStable(FlagAFallRisk(4));

        await Expect(Figure(4, "risk-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Figure(4, "flag-count")).ToHaveTextAsync("5");
        await Expect(FallRiskBadge(4)).ToBeVisibleAsync();
        await Expect(Figure(5, "risk-count")).ToHaveTextAsync("1");
        await Expect(FallRiskBadge(5)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
