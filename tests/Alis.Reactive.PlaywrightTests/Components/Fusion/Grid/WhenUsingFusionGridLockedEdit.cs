namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the front-office records coordinator, I want a discharged resident's record to stay closed when I try to
/// update it, and to be told why, while a current resident's record opens for the family's update, so that the
/// record of a resident who has left stays as it was on the day they left.
/// </summary>
/// <remarks>
/// The records are Amina Patel, Henry Liu, Ruth Carter (discharged September 30), Grace Bennett and Walter Grant
/// (discharged October 2). Two family requests wait: Ruth Carter's son's and Henry Liu's daughter's new phone
/// numbers; each opens the resident's record from its button, and a double-click on a row opens it too.
/// Not exercised:
/// BeginEdit's RowIndex and Type: the page decides by the row's own data; the index is where the row happens to
/// sit, and Type is "edit" on every row edit (normal-edit.js:110), so neither changes what the page shows.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridLockedEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/LockedResidentEdit";
    private const string GridId = "resident-records";

    // The grid numbers its columns from 1, counting the hidden ID column.
    private const int FamilyPhoneColumn = 6;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator FamilyPhone(string resident) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{FamilyPhoneColumn}']");
    private ILocator RowBeingEdited => Page.Locator($"#{GridId} .e-editedrow");
    private ILocator Editor(string field) => Page.Locator($"#{GridId}{field}");
    private ILocator UpdateRuthsRecord => Page.Locator("#update-record-6403");
    private ILocator UpdateHenrysRecord => Page.Locator("#update-record-6402");
    private ILocator SaveRecord => Page.Locator("#record-save");
    private ILocator RecordClosed => Page.Locator("#record-closed");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    // The notice is written while the grid decides whether to open the row, so once it shows, no row can still open.
    [Test]
    public async Task a_discharged_residents_record_stays_closed_and_the_page_says_why()
    {
        await NavigateAndBoot();

        await ClickWhenStable(UpdateRuthsRecord);

        await Expect(RecordClosed).ToHaveTextAsync(
            "Ruth Carter was discharged on September 30. The record is closed; send the family's request to medical records.",
            new() { Timeout = 10000 });
        await Expect(RecordClosed).ToBeVisibleAsync();
        await Expect(RowBeingEdited).ToHaveCountAsync(0);
        await Expect(FamilyPhone("Ruth Carter")).ToHaveTextAsync("555-0164");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_clicking_a_discharged_residents_row_keeps_the_record_closed()
    {
        await NavigateAndBoot();

        await FamilyPhone("Walter Grant").DblClickAsync();

        await Expect(RecordClosed).ToHaveTextAsync(
            "Walter Grant was discharged on October 2. The record is closed; send the family's request to medical records.",
            new() { Timeout = 10000 });
        await Expect(RowBeingEdited).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // Henry is the second row: his request finds his row by his resident number, selects it and opens it.
    [Test]
    public async Task a_current_residents_record_opens_for_the_familys_update()
    {
        await NavigateAndBoot();

        await ClickWhenStable(UpdateHenrysRecord);

        await Expect(Editor("residentName")).ToHaveValueAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(Editor("familyPhone")).ToHaveValueAsync("555-0187");
        await Expect(RecordClosed).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_familys_new_number_is_saved_to_the_record()
    {
        await NavigateAndBoot();
        await ClickWhenStable(UpdateHenrysRecord);
        await Expect(Editor("familyPhone")).ToHaveValueAsync("555-0187", new() { Timeout = 10000 });
        await Editor("familyPhone").FillAsync("555-0240");

        await ClickWhenStable(SaveRecord);

        await Expect(FamilyPhone("Henry Liu")).ToHaveTextAsync("555-0240", new() { Timeout = 10000 });
        await Expect(RowBeingEdited).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_current_record_after_a_closed_one_takes_the_notice_away()
    {
        await NavigateAndBoot();
        await ClickWhenStable(UpdateRuthsRecord);
        await Expect(RecordClosed).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(UpdateHenrysRecord);

        await Expect(Editor("residentName")).ToHaveValueAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(RecordClosed).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
