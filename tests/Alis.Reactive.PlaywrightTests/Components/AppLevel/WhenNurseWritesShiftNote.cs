using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a nurse, I want to write a resident's shift note in their drawer and back out of a discharge
/// with Escape, so that the drawer, and the note I was writing in it, are still there to finish.
/// </summary>
[TestFixture]
public class WhenNurseWritesShiftNote : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer/ShiftNote";

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");
    private ILocator OpenMargaretHill => Page.Locator("#open-margaret-hill-btn");
    private ILocator ShiftNote => Page.Locator($"#{IdGenerator.For<ShiftNoteModel>(m => m.Note)}");
    private ILocator SaveNote => Page.Locator("#save-shift-note-btn");
    private ILocator DischargeResident => Page.Locator("#discharge-resident-btn");
    private ILocator ShiftNoteStatus => Page.Locator("#shift-note-status");

    // The drawer drops this class the moment it starts closing, while its slide-out keeps it
    // painted for a moment, so whether it is open is read from the class rather than from paint.
    private static readonly Regex DrawerOpen = new("alis-drawer--visible");

    private const string ShiftNoteText = "Refused breakfast; ate a full lunch.";

    private async Task OpenMargaretHillsDrawer()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await ClickWhenStable(OpenMargaretHill);
        await Expect(ShiftNote).ToBeVisibleAsync();
    }

    [Test]
    public async Task escaping_the_discharge_question_keeps_the_drawer_and_the_shift_note()
    {
        await OpenMargaretHillsDrawer();
        await ShiftNote.FillAsync(ShiftNoteText);

        await ClickWhenStable(DischargeResident);
        await Expect(ConfirmDialog).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");

        await Expect(ConfirmDialog).ToBeHiddenAsync();
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(ShiftNote).ToHaveValueAsync(ShiftNoteText);
        await ClickWhenStable(SaveNote);
        await Expect(ShiftNoteStatus).ToHaveTextAsync("Shift note saved for Margaret Hill.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_drawer_when_no_question_is_open()
    {
        await OpenMargaretHillsDrawer();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(ShiftNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_discharge_discharges_the_resident()
    {
        await OpenMargaretHillsDrawer();

        await ClickWhenStable(DischargeResident);
        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "OK" }));

        await Expect(ShiftNoteStatus).ToHaveTextAsync("Margaret Hill has been discharged.");
        AssertNoConsoleErrors();
    }
}
