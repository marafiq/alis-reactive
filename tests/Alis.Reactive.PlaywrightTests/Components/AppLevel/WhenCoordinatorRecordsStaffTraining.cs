using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a care coordinator, I want to close a course list, a calendar or a trainer list in the training
/// drawer with Escape, so that the drawer, and the training notes I typed in it, are still there to save.
/// </summary>
[TestFixture]
public class WhenCoordinatorRecordsStaffTraining : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer/StaffTraining";

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator RecordTraining => Page.Locator("#record-training-btn");
    private ILocator SaveTraining => Page.Locator("#save-training-btn");
    private ILocator TrainingStatus => Page.Locator("#training-status");

    private ComponentScope TrainingForm => new(Page, typeof(StaffTrainingRecordModel));
    private ILocator Notes => Page.Locator($"#{TrainingForm.IdFor(nameof(StaffTrainingRecordModel.Notes))}");
    private MultiSelectLocator Courses => TrainingForm.MultiSelect(nameof(StaffTrainingRecordModel.Courses));
    private DatePickerLocator CompletedOn => TrainingForm.DatePicker(nameof(StaffTrainingRecordModel.CompletedOn));
    private DropDownListLocator Trainer => TrainingForm.DropDownList(nameof(StaffTrainingRecordModel.TrainerId));

    // The drawer drops this class the moment it starts closing, while its slide-out keeps it
    // painted for a moment, so whether it is open is read from the class rather than from paint.
    private static readonly Regex DrawerOpen = new("alis-drawer--visible");

    private const string TrainingNotes = "Practised two-person transfers with Mr. Alvarez.";

    private async Task OpenTheTrainingDrawerAndTypeNotes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await ClickWhenStable(RecordTraining);
        await Expect(Notes).ToBeVisibleAsync();
        await Notes.FillAsync(TrainingNotes);
    }

    private async Task ExpectTheDrawerStillHoldsTheNotes()
    {
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToHaveValueAsync(TrainingNotes);
    }

    [Test]
    public async Task escaping_the_course_list_keeps_the_drawer_and_the_notes_to_save()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await Courses.Open();
        await Courses.PopupItem("Fall prevention").ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Courses.Popup).ToBeHiddenAsync();
        await ExpectTheDrawerStillHoldsTheNotes();
        await ClickWhenStable(SaveTraining);
        await Expect(TrainingStatus).ToHaveTextAsync($"Training recorded: {TrainingNotes}");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escaping_the_calendar_keeps_the_drawer_and_the_notes()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(CompletedOn.CalendarIcon);
        await Expect(CompletedOn.Popup).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(CompletedOn.Popup).ToBeHiddenAsync();
        await ExpectTheDrawerStillHoldsTheNotes();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_on_the_date_closes_the_drawer_once_the_calendar_is_closed()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(CompletedOn.CalendarIcon);
        await Expect(CompletedOn.Popup).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(CompletedOn.Popup).ToBeHiddenAsync();
        await Expect(CompletedOn.Input).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escaping_the_trainer_list_keeps_the_drawer_and_the_notes()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await Trainer.Open();
        await Expect(Trainer.Popup).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Trainer.Popup).ToBeHiddenAsync();
        await ExpectTheDrawerStillHoldsTheNotes();
        AssertNoConsoleErrors();
    }
}
