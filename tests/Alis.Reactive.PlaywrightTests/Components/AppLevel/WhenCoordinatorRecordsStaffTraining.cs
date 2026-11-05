using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a care coordinator, I want Escape to leave the training drawer open, so that closing a course list
/// or pressing it by habit never throws away the training notes I typed; and I want Escape to close the
/// training history drawer, which holds nothing to lose.
/// </summary>
[TestFixture]
public class WhenCoordinatorRecordsStaffTraining : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer/StaffTraining";

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator DrawerTitle => Page.Locator("#alis-drawer-title");
    private ILocator CloseDrawer => Page.Locator("#alis-drawer-close");
    private ILocator RecordTraining => Page.Locator("#record-training-btn");
    private ILocator TrainingHistoryButton => Page.Locator("#training-history-btn");
    private ILocator SaveTraining => Page.Locator("#save-training-btn");
    private ILocator TrainingStatus => Page.Locator("#training-status");
    private ILocator TrainingHistory => Page.Locator("#training-history");

    private ComponentScope TrainingForm => new(Page, typeof(StaffTrainingRecordModel));
    private ILocator Notes => Page.Locator($"#{TrainingForm.IdFor(nameof(StaffTrainingRecordModel.Notes))}");
    private MultiSelectLocator Courses => TrainingForm.MultiSelect(nameof(StaffTrainingRecordModel.Courses));

    private static readonly Regex DrawerOpen = new("alis-drawer--visible");

    private const string TrainingNotes = "Practised two-person transfers with Mr. Alvarez.";

    private async Task OpenTheTrainingDrawerAndTypeNotes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await ClickWhenStable(RecordTraining);
        await Notes.FillAsync(TrainingNotes);
    }

    private async Task OpenTheTrainingHistory() => await ClickWhenStable(TrainingHistoryButton);

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
    public async Task the_close_button_still_closes_the_training_drawer()
    {
        await OpenTheTrainingDrawerAndTypeNotes();

        await ClickWhenStable(CloseDrawer);

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Notes).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_training_history()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await OpenTheTrainingHistory();
        await Expect(TrainingHistory).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(TrainingHistory).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_history_opened_after_the_close_button_closed_the_training_drawer()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(CloseDrawer);
        await OpenTheTrainingHistory();
        await Expect(DrawerTitle).ToHaveTextAsync("Training History");
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_closes_the_history_opened_after_saving_closed_the_training_drawer()
    {
        await OpenTheTrainingDrawerAndTypeNotes();
        await ClickWhenStable(SaveTraining);
        await Expect(TrainingStatus).ToHaveTextAsync($"Training recorded: {TrainingNotes}");
        await OpenTheTrainingHistory();
        await Expect(DrawerTitle).ToHaveTextAsync("Training History");
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);

        await Page.Keyboard.PressAsync("Escape");

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        AssertNoConsoleErrors();
    }
}
