using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

/// <summary>
/// As a user managing my task list
/// I want to create todo items with optional urgency and due dates
/// So that I can track what needs to be done and when
/// </summary>
[TestFixture]
public class WhenManagingTodoItems : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/Todo";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TodoModel__";

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
    private ILocator UrgentCheckbox => Page.Locator($"#{Scope}IsUrgent");

    // ── Page Load ──

    [Test]
    public async Task page_loads_with_empty_form_and_save_button()
    {
        await NavigateAndBoot();

        await Expect(_plan.TextBox(m => m.Title).Input).ToHaveValueAsync("");
        await Expect(SaveBtn).ToBeVisibleAsync();
        await Expect(Result).ToContainTextAsync("Fill in the form and click Save");
        AssertNoConsoleErrors();
    }

    // ── Validation — Required Title ──

    [Test]
    public async Task submitting_empty_title_shows_required_error()
    {
        await NavigateAndBoot();

        await SaveBtn.ClickAsync();

        await Expect(_plan.ErrorFor(m => m.Title))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Conditional Visibility — Due Date ──

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

    // ── Validation — Conditional Due Date ──

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

    // ── Successful Submission ──

    [Test]
    public async Task saving_valid_todo_shows_success_message()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.Title).FillAndBlur("Buy groceries");

        await SaveBtn.ClickAsync();

        // Success shows a toast notification
        await Expect(Page.Locator(".e-toast").First)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
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

        // Success shows a toast notification
        await Expect(Page.Locator(".e-toast").First)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
