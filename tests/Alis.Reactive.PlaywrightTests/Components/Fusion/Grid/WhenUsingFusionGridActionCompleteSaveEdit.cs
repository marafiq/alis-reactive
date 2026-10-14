using System.Collections.Concurrent;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the resident services coordinator, I want a name I correct on the Maple wing roster to reach Riverside
/// Pharmacy as soon as it is saved, with the old and the new name, and no other change to bother the pharmacy,
/// so that the labels on a resident's medications always carry the name on the roster.
/// </summary>
/// <remarks>
/// The roster is Amina Patel, Grace Benett and Henry Lui (both keyed in wrong at move-in) and Irene Morgan.
/// The coordinator double-clicks a cell to correct it and presses Enter to save; the pharmacy's answer names
/// the resident's line on the printed roster.
/// Not exercised:
/// ActionComplete's Action: every save on this roster is an edit; the Maple wing roster
/// (WhenUsingFusionGridEditing) tells an added resident from an edited one by it.
/// ActionComplete's Name and Type: EJ2 sets both to "actionComplete" (observer.js:92; normal-edit.js:421) on
/// every save, so they carry nothing a page could show.
/// ActionComplete's SelectedRow: an inline save reports -1, as the roster clears its selection when a row
/// opens for editing (normal-edit.js:227-229, 300).
/// ActionComplete's Cancel: EJ2 sends false once an action has completed; the Maple wing roster writes it in
/// ActionBegin.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridActionCompleteSaveEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ActionCompleteSaveEdit";
    private const string RosterId = "pharmacy-roster";

    // The roster numbers its columns from 1.
    private const int ResidentColumn = 2;
    private const int NurseColumn = 4;

    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator Editor(string field) => Page.Locator($"#{RosterId}{field}");
    private ILocator PharmacyNotice => Page.Locator("#pharmacy-notice");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    // Double-clicks the resident's cell, writes over what it holds and presses Enter to save the row.
    private async Task Correct(string resident, int column, string field, string current, string corrected)
    {
        await Cell(resident, column).DblClickAsync();
        await Expect(Editor(field)).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await Editor(field).FillAsync(corrected);
        await Editor(field).PressAsync("Enter");
    }

    [Test]
    public async Task a_corrected_name_reaches_the_pharmacy_with_the_old_and_new_name()
    {
        await NavigateAndBoot();

        await Correct("Henry Lui", ResidentColumn, "residentName", "Henry Lui", "Henry Liu");

        await Expect(Cell("Henry Liu", ResidentColumn)).ToHaveTextAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(PharmacyNotice).ToHaveTextAsync(
            "Riverside Pharmacy has the corrected name: Henry Lui is now Henry Liu (Maple roster line 3).",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // A nurse change is saved first and sends nothing; the name correction after it is the one notice sent.
    [Test]
    public async Task a_saved_change_that_keeps_the_name_sends_the_pharmacy_nothing()
    {
        var notices = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Grid/ActionCompleteSaveEditNotice")) notices.Enqueue(request.Url);
        };
        await NavigateAndBoot();

        await Correct("Henry Lui", NurseColumn, "primaryNurse", "Elena Ruiz", "Owen Park");
        await Expect(Cell("Henry Lui", NurseColumn)).ToHaveTextAsync("Owen Park", new() { Timeout = 10000 });
        await Expect(PharmacyNotice).ToHaveTextAsync("No name corrections sent to the pharmacy yet.");

        await Correct("Grace Benett", ResidentColumn, "residentName", "Grace Benett", "Grace Bennett");

        await Expect(PharmacyNotice).ToHaveTextAsync(
            "Riverside Pharmacy has the corrected name: Grace Benett is now Grace Bennett (Maple roster line 2).",
            new() { Timeout = 10000 });
        Assert.That(notices, Has.Count.EqualTo(1), "Only the name correction is sent to the pharmacy.");
        AssertNoConsoleErrors();
    }
}
