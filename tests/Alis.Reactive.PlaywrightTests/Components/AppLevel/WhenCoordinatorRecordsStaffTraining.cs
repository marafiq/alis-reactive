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
    private ILocator DrawerTitle => Page.Locator("#alis-drawer-title");
    private ILocator CloseDrawer => Page.Locator("#alis-drawer-close");
    private ILocator RecordTraining => Page.Locator("#record-training-btn");
    private ILocator TrainingHistoryButton => Page.Locator("#training-history-btn");
    private ILocator SaveTraining => Page.Locator("#save-training-btn");
    private ILocator TrainingStatus => Page.Locator("#training-status");
    private ILocator TrainingHistory => Page.Locator("#training-history");
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

    private async Task OpenTheTrainingHistory()
    {
        await ClickWhenStable(TrainingHistoryButton);
        await Expect(DrawerTitle).ToHaveTextAsync("Training History");
        await Expect(TrainingHistory).ToBeVisibleAsync();
    }

    [Test]
    public async Task escaping_the_course_list_keeps_the_drawer_and_the_notes_to_save()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await Courses.Open();
        await Courses.PopupItem("Fall prevention").ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Courses.Popup).ToBeHiddenAsync();
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToHaveValueAsync(TrainingNotes);
        await ClickWhenStable(SaveTraining);
        await Expect(TrainingStatus).ToHaveTextAsync($"Training recorded: {TrainingNotes}");
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
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
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToHaveValueAsync(TrainingNotes);
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
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToHaveValueAsync(TrainingNotes);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_with_nothing_open_keeps_the_drawer_and_the_notes()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await Expect(Notes).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToHaveValueAsync(TrainingNotes);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_close_button_closes_the_training_drawer_and_the_training_is_not_recorded()
    {
        await OpenTheTrainingDrawerAndTypeNotes();

        await ClickWhenStable(CloseDrawer);

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).Not.ToBeAttachedAsync();
        await Expect(TrainingStatus).ToHaveTextAsync("Training not recorded.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_training_history()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await OpenTheTrainingHistory();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(TrainingHistory).Not.ToBeAttachedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_history_opened_after_the_close_button_closed_the_training_drawer()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(CloseDrawer);
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await OpenTheTrainingHistory();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(TrainingHistory).Not.ToBeAttachedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_history_opened_after_saving_closed_the_training_drawer()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(SaveTraining);
        await Expect(TrainingStatus).ToHaveTextAsync($"Training recorded: {TrainingNotes}");
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await OpenTheTrainingHistory();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(TrainingHistory).Not.ToBeAttachedAsync();
        AssertNoConsoleErrors();
    }
}
