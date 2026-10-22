using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Patterns.Workflows;

[TestFixture]
public class WhenManagingTodoItems : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/Todo";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TodoModel__";

    private PagePlan<TodoModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<TodoModel>.FromPage(Page);
    }

    private ILocator SaveBtn => Page.Locator("#save-btn");
    private ILocator Result => _plan.Element("todo-result");
    private ILocator DueDateSection => Page.Locator("#due-date-section");
    private ILocator UrgentCheckbox => Page.Locator($"#{ModelIdPrefix}IsUrgent");
    private ILocator SaveToast => Page.Locator(".e-toast").First;

    [Test]
    public async Task page_loads_with_empty_form_and_save_button()
    {
        await NavigateAndBoot();

        await Expect(_plan.TextBox(m => m.Title).Input).ToHaveValueAsync("");
        await Expect(SaveBtn).ToBeVisibleAsync();
        await Expect(Result).ToContainTextAsync("Fill in the form and click Save");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submitting_empty_title_shows_required_error()
    {
        await NavigateAndBoot();

        await SaveBtn.ClickAsync();

        await Expect(_plan.ErrorFor(m => m.Title))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_urgent_reveals_due_date_field()
    {
        await NavigateAndBoot();

        await Expect(DueDateSection).ToBeHiddenAsync();

        await UrgentCheckbox.CheckAsync();

        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecking_urgent_hides_due_date_field()
    {
        await NavigateAndBoot();

        await UrgentCheckbox.CheckAsync();
        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });

        await UrgentCheckbox.UncheckAsync();

        await Expect(DueDateSection).ToBeHiddenAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task urgent_todo_without_due_date_shows_required_error()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.Title).FillAndBlur("Urgent task");
        await UrgentCheckbox.CheckAsync();
        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });

        await SaveBtn.ClickAsync();

        await Expect(_plan.ErrorFor(m => m.DueDate))
            .ToContainTextAsync("Urgent todos need a due date", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_valid_todo_shows_success_message()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.Title).FillAndBlur("Buy groceries");

        await SaveBtn.ClickAsync();

        await Expect(SaveToast)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The layout's toast host asks Syncfusion for the bottom-right corner. A global Newtonsoft
    // naming policy that renames Syncfusion's pinned "X"/"Y" drops that request and the toast
    // falls back to the top left, over the page header.
    [Test]
    public async Task save_confirmation_appears_in_the_bottom_right_corner()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.Title).FillAndBlur("Buy groceries");

        await SaveBtn.ClickAsync();

        await Expect(SaveToast)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
        var toast = (await SaveToast.BoundingBoxAsync())!;
        var window = Page.ViewportSize!;
        var toastCenterX = toast.X + toast.Width / 2;
        var toastCenterY = toast.Y + toast.Height / 2;
        Assert.That(toastCenterX, Is.GreaterThan(window.Width / 2), "confirmation sits on the right half of the window");
        Assert.That(toastCenterY, Is.GreaterThan(window.Height / 2), "confirmation sits on the bottom half of the window");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_urgent_todo_with_due_date_succeeds()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.Title).FillAndBlur("File quarterly report");
        await UrgentCheckbox.CheckAsync();
        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });
        await _plan.DatePicker(m => m.DueDate).FillAndBlur("12/31/2026");

        await SaveBtn.ClickAsync();

        await Expect(SaveToast)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
