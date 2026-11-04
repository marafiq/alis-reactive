namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridLockedEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/LockedResidentEdit";
    private const string GridId = "resident-records";

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
