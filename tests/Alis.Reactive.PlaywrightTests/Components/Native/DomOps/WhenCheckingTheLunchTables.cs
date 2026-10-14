namespace Alis.Reactive.PlaywrightTests.Components.Native.DomOps;

/// <summary>
/// As the dining room supervisor before lunch service, I want each table card to tell me who is seated,
/// who needs a staff member close by, which plates the kitchen restricts in sodium and the table's safety
/// risks, and to count a fall risk the moment I flag it, so that every resident gets the plate and the
/// supervision their care plan asks for.
/// </summary>
/// <remarks>
/// The page reads each table off the card the server renders: its flags are the card's classes and its
/// residents are the children of its seat list, each carrying its diet and supervision as attributes.
/// Table 4 seats four: Margaret Hale (low sodium) and Evelyn Hart (reduced sodium) under close supervision,
/// Walter Brooks (sodium restricted) and Harold Kim (diabetic) on standby. Its flags are a choking risk, a
/// food allergy, memory care and "choking-risk-reviewed", which holds "risk-" after its start. Table 5 seats
/// three, one under close supervision, and flags a food allergy, wheelchair access and
/// "allergy-risk-reviewed"; it is not a memory care table. Every class-list, child-list and attribute read
/// the page makes is exercised.
/// </remarks>
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

    // Table 4 has two residents on close supervision and two on standby, so only Table 5 (one of three)
    // tells == from !=.
    [Test]
    public async Task each_table_card_counts_the_residents_who_need_close_supervision()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "close-supervision")).ToHaveTextAsync("2");
        await Expect(Figure(5, "close-supervision")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    // "low-sodium", "sodium-restricted", "reduced-sodium" and "diabetic": every diet naming sodium is a
    // restricted plate (3). A StartsWith would count 1, an EndsWith 2, no test at all 4.
    [Test]
    public async Task every_diet_that_names_sodium_is_a_sodium_restricted_plate()
    {
        await OpenTheSeating();

        await Expect(Figure(4, "sodium-plates")).ToHaveTextAsync("3");
        await Expect(Figure(5, "sodium-plates")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    // Only the low and reduced sodium diets end in "sodium" (2); a Contains would add Walter Brooks's
    // sodium-restricted plate (3), a StartsWith would count only his (1).
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

    // Only flags that start "risk-" are safety risks: "choking-risk-reviewed" and "allergy-risk-reviewed"
    // hold "risk-" after their start, so a Contains would count 3 and 2.
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

    // The flag is added to the card's classes, and the same click counts the classes again: the count
    // reads the class just added, at once.
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
