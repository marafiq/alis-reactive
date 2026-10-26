using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.MedicationList;

/// <summary>
/// As the admitting nurse, I want a new resident's medication list counted, totalled, ordered and searched
/// as I reconcile it line by line against the pharmacy's delivery, so that nothing delivered is missed and
/// nothing missing is given.
/// </summary>
/// <remarks>
/// The list is a widget the sandbox wrote itself (Scripts/medication-list.ts, BroughtComponents/
/// MedicationList): a list of objects with members of their own, read as data for the figures and as
/// objects for the lines' names and units. The pharmacy's delivery is the server's answer, read the same
/// way. Margaret Hale's list holds five lines: one controlled (lorazepam), one injection (insulin, the
/// earliest dose at 07:00), one already reconciled (the lidocaine patch, the latest dose at 20:00), two
/// starting with L, one of ten units or more; the delivery holds four of them, without the patch, and its
/// warfarin label carries a trailing space. Every figure is worked out in the browser, so a dropped
/// filter, a wrong order, a sum that concatenates, or a search that ignores case or spaces each shows a
/// different figure; the empty list and the empty delivery show every figure at nothing.
/// </remarks>
[TestFixture]
public class WhenReconcilingAMedicationList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MedicationList";

    private const string ListNamesAtoZ = "Insulin glargine,Lidocaine patch,Lorazepam 0.5 mg,Metoprolol 25 mg,Warfarin 2 mg";
    private const string DeliveryNamesAtoZ = "Insulin glargine,Lorazepam 0.5 mg,Metoprolol 25 mg,Warfarin 2 mg ";

    private ILocator List => Page.Locator($"#{MedicationReconciliationModel.ListId}");
    private ILocator ReconcileAll => Page.Locator("#reconcile-all");
    private ILocator AddTheAspirin => Page.Locator("#add-aspirin");
    private ILocator RecordTheEmptyDelivery => Page.Locator("#load-empty-delivery");

    private ILocator Figure(string id) => Page.Locator($"#{id}");

    private ILocator Reconciled(string medication) => List.GetByLabel($"{medication} reconciled", new() { Exact = true });

    private ILocator Remove(string medication) => List.GetByRole(AriaRole.Button, new() { Name = $"Remove {medication}", Exact = true });

    private ILocator ClearTheList => List.GetByRole(AriaRole.Button, new() { Name = "Clear the list", Exact = true });

    private async Task OpenTheList()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Figure("list-count")).ToHaveTextAsync("5");
        await Expect(Figure("delivery-count")).ToHaveTextAsync("4", new() { Timeout = 5000 });
    }

    // ── the list: a brought component's list of objects ─────────────────────

    [Test]
    public async Task the_list_opens_counted_and_totalled_in_units_cost_and_hours()
    {
        await OpenTheList();

        await Expect(Figure("list-size")).ToHaveTextAsync("5");
        await Expect(Figure("list-controlled")).ToHaveTextAsync("1");
        await Expect(Figure("list-units")).ToHaveTextAsync("25");
        await Expect(Figure("list-cost")).ToHaveTextAsync("20");
        await Expect(Figure("list-hours")).ToHaveTextAsync("80");
        await Expect(Figure("list-any")).ToHaveTextAsync("Lines to reconcile.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_delivery_is_counted_and_totalled_from_the_pharmacys_lines()
    {
        await OpenTheList();

        await Expect(Figure("delivery-controlled")).ToHaveTextAsync("1");
        await Expect(Figure("delivery-units")).ToHaveTextAsync("24");
        await Expect(Figure("delivery-cost")).ToHaveTextAsync("16.5");
        await Expect(Figure("delivery-hours")).ToHaveTextAsync("68");
        await Expect(Figure("delivery-any")).ToHaveTextAsync("Lines to reconcile.");
        AssertNoConsoleErrors();
    }

    [Test]
    [Ignore("Framework defect: an enum constant captured inside an array lambda (l.Route == MedicationRoute.Injection) is written as its integer, lit 1 under shape string, while the lines hold the enum's JavaScript text \"injection\", so the predicate never matches (ElementExpressionCompiler.cs:156-157 writes it with LiteralFromValue, which skips LiteralFor's enum case at ValueExpression.cs:50).")]
    public async Task an_injection_on_the_list_means_a_nurse_gives_it_until_it_is_removed()
    {
        await OpenTheList();
        await Expect(Figure("list-injection")).ToHaveTextAsync("An injection: a nurse gives it.");
        await Expect(Figure("delivery-injection")).ToHaveTextAsync("An injection: a nurse gives it.");

        await Remove("Insulin glargine").ClickAsync();

        await Expect(Figure("list-injection")).ToHaveTextAsync("No injections.");
        await Expect(Figure("list-count")).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_controlled_medication_is_found_and_counted_with_a_witness_until_it_is_removed()
    {
        await OpenTheList();
        await Expect(Figure("list-controlled-first")).ToHaveTextAsync("A controlled medication is on the list: count it with a witness.");
        await Expect(Figure("delivery-controlled-first")).ToHaveTextAsync("A controlled medication is on the list: count it with a witness.");

        await Remove("Lorazepam 0.5 mg").ClickAsync();

        await Expect(Figure("list-controlled-first")).ToHaveTextAsync("No controlled medication.");
        await Expect(Figure("list-controlled")).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    [Ignore("Framework defect: an enum constant captured inside an array lambda (l.Route == MedicationRoute.Injection) is written as its integer, lit 1 under shape string, while the lines hold the enum's JavaScript text \"injection\", so the predicate never matches (ElementExpressionCompiler.cs:156-157 writes it with LiteralFromValue, which skips LiteralFor's enum case at ValueExpression.cs:50).")]
    public async Task the_first_injection_is_named_and_none_leaves_the_line_blank()
    {
        await OpenTheList();
        await Expect(Figure("list-first-injection")).ToHaveTextAsync("Insulin glargine");
        await Expect(Figure("delivery-first-injection")).ToHaveTextAsync("Insulin glargine");

        await Remove("Insulin glargine").ClickAsync();

        await Expect(Figure("list-first-injection")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_lines_are_named_a_to_z_on_the_list_and_in_the_delivery()
    {
        await OpenTheList();

        await Expect(Figure("list-names")).ToHaveTextAsync(ListNamesAtoZ);
        await Expect(Figure("delivery-names")).ToHaveTextAsync(DeliveryNamesAtoZ);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_lines_are_named_dearest_first_on_the_list_and_in_the_delivery()
    {
        await OpenTheList();

        await Expect(Figure("list-by-cost")).ToHaveTextAsync("Insulin glargine,Lidocaine patch,Lorazepam 0.5 mg,Warfarin 2 mg,Metoprolol 25 mg");
        await Expect(Figure("delivery-by-cost")).ToHaveTextAsync("Insulin glargine,Lorazepam 0.5 mg,Warfarin 2 mg ,Metoprolol 25 mg");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_earliest_and_latest_doses_are_named_by_their_times()
    {
        await OpenTheList();

        await Expect(Figure("list-earliest")).ToHaveTextAsync("Insulin glargine");
        await Expect(Figure("list-latest")).ToHaveTextAsync("Lidocaine patch");
        await Expect(Figure("delivery-earliest")).ToHaveTextAsync("Insulin glargine");
        await Expect(Figure("delivery-latest")).ToHaveTextAsync("Warfarin 2 mg");
        AssertNoConsoleErrors();
    }

    // Three doses fall before noon; the aspirin the page adds is due at 08:00 and makes four.
    [Test]
    public async Task the_doses_before_noon_are_counted_and_an_added_morning_dose_joins_them()
    {
        await OpenTheList();
        await Expect(Figure("list-morning")).ToHaveTextAsync("3");
        await Expect(Figure("delivery-morning")).ToHaveTextAsync("3");

        await AddTheAspirin.ClickAsync();

        await Expect(Figure("list-morning")).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_lines_starting_with_l_are_counted_on_the_list_and_in_the_delivery()
    {
        await OpenTheList();

        await Expect(Figure("list-starting-l")).ToHaveTextAsync("2");
        await Expect(Figure("delivery-starting-l")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task one_line_is_ten_units_or_more_and_two_are_two_units_or_more()
    {
        await OpenTheList();

        await Expect(Figure("list-high-dose")).ToHaveTextAsync("1");
        await Expect(Figure("list-at-least")).ToHaveTextAsync("2");
        await Expect(Figure("delivery-high-dose")).ToHaveTextAsync("1");
        await Expect(Figure("delivery-at-least")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    // The pharmacy's label reads "Warfarin 2 mg " with a trailing space: found once trimmed, not as written.
    [Test]
    public async Task the_pharmacys_padded_warfarin_is_found_once_trimmed_and_not_as_written()
    {
        await OpenTheList();

        await Expect(Figure("delivery-trimmed")).ToHaveTextAsync("1");
        await Expect(Figure("delivery-warfarin")).ToHaveTextAsync("No warfarin as written.");
        await Expect(Figure("list-trimmed")).ToHaveTextAsync("1");
        await Expect(Figure("list-warfarin")).ToHaveTextAsync("Warfarin, as written.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_lidocaine_patch_is_on_the_list_and_missing_from_the_delivery()
    {
        await OpenTheList();

        await Expect(Figure("list-lidocaine")).ToHaveTextAsync("Lidocaine patch on hand.");
        await Expect(Figure("delivery-lidocaine")).ToHaveTextAsync("No lidocaine patch.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lorazepam_on_the_list_locks_the_cart_until_it_is_removed()
    {
        await OpenTheList();
        await Expect(Figure("list-lorazepam")).ToHaveTextAsync("A benzodiazepine on the list: lock it in the cart.");

        await Remove("Lorazepam 0.5 mg").ClickAsync();

        await Expect(Figure("list-lorazepam")).ToHaveTextAsync("No benzodiazepine on the list.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_list_names_its_lines_as_the_objects_hold_them_and_sums_their_units_line_by_line()
    {
        await OpenTheList();

        await Expect(Figure("list-line-names")).ToHaveTextAsync("Metoprolol 25 mg,Warfarin 2 mg,Lorazepam 0.5 mg,Insulin glargine,Lidocaine patch");
        await Expect(Figure("list-units-each")).ToHaveTextAsync("25");

        await Remove("Insulin glargine").ClickAsync();

        await Expect(Figure("list-line-names")).ToHaveTextAsync("Metoprolol 25 mg,Warfarin 2 mg,Lorazepam 0.5 mg,Lidocaine patch");
        await Expect(Figure("list-units-each")).ToHaveTextAsync("5");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task maple_grove_pharmacy_delivered_and_the_empty_delivery_names_no_pharmacy()
    {
        await OpenTheList();
        await Expect(Figure("delivery-pharmacy")).ToHaveTextAsync("Maple Grove Pharmacy delivered.");
        await Expect(Figure("delivery-pharmacies")).ToHaveTextAsync("Maple Grove Pharmacy");

        await RecordTheEmptyDelivery.ClickAsync();

        await Expect(Figure("delivery-pharmacy")).ToHaveTextAsync("No delivery from Maple Grove Pharmacy.", new() { Timeout = 5000 });
        await Expect(Figure("delivery-pharmacies")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_a_line_reconciled_counts_it_and_marking_every_line_completes_the_reconciliation()
    {
        await OpenTheList();
        await Expect(Figure("list-all")).ToHaveTextAsync("Lines still to reconcile.");

        await Reconciled("Lorazepam 0.5 mg").CheckAsync();
        await Expect(Figure("changed-line")).ToHaveTextAsync("Lorazepam 0.5 mg");
        await Expect(Figure("changed-reconciled")).ToHaveTextAsync("2");
        await Expect(Figure("list-all")).ToHaveTextAsync("Lines still to reconcile.");

        await ReconcileAll.ClickAsync();
        await Expect(Figure("list-all")).ToHaveTextAsync("Every line reconciled.");
        await Expect(Figure("changed-reconciled")).ToHaveTextAsync("5");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_emptied_list_has_nothing_to_count_order_find_or_reconcile()
    {
        await OpenTheList();

        await ClearTheList.ClickAsync();

        await Expect(Figure("list-count")).ToHaveTextAsync("0");
        await Expect(Figure("list-size")).ToHaveTextAsync("0");
        await Expect(Figure("list-controlled")).ToHaveTextAsync("0");
        await Expect(Figure("list-units")).ToHaveTextAsync("0");
        await Expect(Figure("list-units-each")).ToHaveTextAsync("0");
        await Expect(Figure("list-cost")).ToHaveTextAsync("0");
        await Expect(Figure("list-hours")).ToHaveTextAsync("0");
        await Expect(Figure("list-any")).ToHaveTextAsync("Nothing on the list.");
        await Expect(Figure("list-injection")).ToHaveTextAsync("No injections.");
        await Expect(Figure("list-all")).ToHaveTextAsync("Nothing to reconcile.");
        await Expect(Figure("list-controlled-first")).ToHaveTextAsync("No controlled medication.");
        await Expect(Figure("list-first-injection")).ToHaveTextAsync("");
        await Expect(Figure("list-names")).ToHaveTextAsync("");
        await Expect(Figure("list-by-cost")).ToHaveTextAsync("");
        await Expect(Figure("list-earliest")).ToHaveTextAsync("");
        await Expect(Figure("list-latest")).ToHaveTextAsync("");
        await Expect(Figure("list-morning")).ToHaveTextAsync("0");
        await Expect(Figure("list-starting-l")).ToHaveTextAsync("0");
        await Expect(Figure("list-high-dose")).ToHaveTextAsync("0");
        await Expect(Figure("list-at-least")).ToHaveTextAsync("0");
        await Expect(Figure("list-trimmed")).ToHaveTextAsync("0");
        await Expect(Figure("list-lidocaine")).ToHaveTextAsync("No lidocaine patch.");
        await Expect(Figure("list-warfarin")).ToHaveTextAsync("No warfarin as written.");
        await Expect(Figure("list-lorazepam")).ToHaveTextAsync("No benzodiazepine on the list.");
        await Expect(Figure("list-line-names")).ToHaveTextAsync("");
        await Expect(Figure("changed-count")).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_empty_delivery_counts_orders_and_finds_nothing()
    {
        await OpenTheList();

        await RecordTheEmptyDelivery.ClickAsync();

        await Expect(Figure("delivery-count")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Figure("delivery-controlled")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-units")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-cost")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-hours")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-any")).ToHaveTextAsync("Nothing on the list.");
        await Expect(Figure("delivery-injection")).ToHaveTextAsync("No injections.");
        await Expect(Figure("delivery-all")).ToHaveTextAsync("Nothing to reconcile.");
        await Expect(Figure("delivery-controlled-first")).ToHaveTextAsync("No controlled medication.");
        await Expect(Figure("delivery-first-injection")).ToHaveTextAsync("");
        await Expect(Figure("delivery-names")).ToHaveTextAsync("");
        await Expect(Figure("delivery-by-cost")).ToHaveTextAsync("");
        await Expect(Figure("delivery-earliest")).ToHaveTextAsync("");
        await Expect(Figure("delivery-latest")).ToHaveTextAsync("");
        await Expect(Figure("delivery-morning")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-starting-l")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-high-dose")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-at-least")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-trimmed")).ToHaveTextAsync("0");
        await Expect(Figure("delivery-lidocaine")).ToHaveTextAsync("No lidocaine patch.");
        await Expect(Figure("delivery-warfarin")).ToHaveTextAsync("No warfarin as written.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_daily_aspirin_from_the_page_puts_it_first_a_to_z()
    {
        await OpenTheList();

        await AddTheAspirin.ClickAsync();

        await Expect(Figure("changed-line")).ToHaveTextAsync("Aspirin 81 mg");
        await Expect(Figure("changed-count")).ToHaveTextAsync("6");
        await Expect(Figure("list-count")).ToHaveTextAsync("6");
        await Expect(Figure("list-names")).ToHaveTextAsync("Aspirin 81 mg," + ListNamesAtoZ);
        await Expect(List.Locator("tr.ml-line .ml-route").Last).ToHaveTextAsync("oral");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_line_added_on_the_list_itself_is_counted_like_any_other()
    {
        await OpenTheList();

        await List.GetByLabel("New medication", new() { Exact = true }).FillAsync("Vitamin D 1000 IU");
        await List.GetByLabel("New medication units", new() { Exact = true }).FillAsync("1");
        await List.GetByLabel("New medication route", new() { Exact = true }).SelectOptionAsync(new SelectOptionValue { Label = "By mouth" });
        await List.GetByRole(AriaRole.Button, new() { Name = "Add to the list", Exact = true }).ClickAsync();

        await Expect(Figure("changed-line")).ToHaveTextAsync("Vitamin D 1000 IU");
        await Expect(Figure("list-count")).ToHaveTextAsync("6");
        await Expect(Figure("list-units")).ToHaveTextAsync("26");
        await Expect(Figure("list-latest")).ToHaveTextAsync("Lidocaine patch");
        AssertNoConsoleErrors();
    }
}
