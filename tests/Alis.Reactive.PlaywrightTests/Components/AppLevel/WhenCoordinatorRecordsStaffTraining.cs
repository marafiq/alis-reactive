using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

[TestFixture]
public class WhenCoordinatorRecordsStaffTraining : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer/StaffTraining";
    private const string TrainingNotes = "Practised two-person transfers with Mr. Alvarez.";

    private static readonly Regex DrawerOpen = new("alis-drawer--visible");

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator RecordTraining => Page.Locator("#record-training-btn");
    private ILocator SaveTraining => Page.Locator("#save-training-btn");
    private ILocator TrainingStatus => Page.Locator("#training-status");
    private ILocator Notes => Page.Locator($"#{IdGenerator.For<StaffTrainingRecordModel>(m => m.Notes)}");
    private MultiSelectLocator Courses => new(Page, IdGenerator.For<StaffTrainingRecordModel>(m => m.Courses));
    private DatePickerLocator CompletedOn => new(Page, IdGenerator.For<StaffTrainingRecordModel>(m => m.CompletedOn));
    private DropDownListLocator Trainer => new(Page, IdGenerator.For<StaffTrainingRecordModel>(m => m.TrainerId));

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
        await Expect(Notes).Not.ToBeAttachedAsync();
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

    [Test]
    public async Task escape_in_the_notes_with_no_list_open_closes_the_drawer_and_clears_it()
    {
        await OpenTheTrainingDrawerAndTypeNotes();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).Not.ToBeAttachedAsync();
        AssertNoConsoleErrors();
    }
}
