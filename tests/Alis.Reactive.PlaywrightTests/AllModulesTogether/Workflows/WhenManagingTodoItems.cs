using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

[TestFixture]
public class WhenManagingTodoItems : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/Todo";
    private TodoPage _page = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        _page = new TodoPage(Page);
    }

    private ILocator SaveBtn => Page.Locator("#save-btn");
    private ILocator Result => _page.Result;
    private ILocator DueDateSection => Page.Locator("#due-date-section");
    private ILocator UrgentCheckbox => Page.Locator("#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TodoModel__IsUrgent");

    // ── Page Load ──

    [Test]
    public async Task page_loads_with_empty_form_and_save_button()
    {
        await NavigateAndBoot();

        await Expect(_page.Title.Input).ToHaveValueAsync("");
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

        await Expect(_page.TitleError)
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

        await _page.Title.FillAndBlur("Urgent task");
        await UrgentCheckbox.CheckAsync();
        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });

        await SaveBtn.ClickAsync();

        await Expect(_page.DueDateError)
            .ToContainTextAsync("Urgent todos need a due date", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Successful Submission ──

    [Test]
    public async Task saving_valid_todo_shows_success_message()
    {
        await NavigateAndBoot();

        await _page.Title.FillAndBlur("Buy groceries");

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

        await _page.Title.FillAndBlur("File quarterly report");
        await UrgentCheckbox.CheckAsync();
        await Expect(DueDateSection).ToBeVisibleAsync(new() { Timeout = 3000 });
        await _page.DueDate.FillAndBlur("12/31/2026");

        await SaveBtn.ClickAsync();

        // Success shows a toast notification
        await Expect(Page.Locator(".e-toast").First)
            .ToContainTextAsync("Todo saved successfully", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    private sealed class TodoPage
    {
        private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TodoModel__";
        private readonly IPage _page;

        public TodoPage(IPage page)
        {
            _page = page;
        }

        public NativeTextBoxLocator Title => new(_page, Scope + "Title");
        public DatePickerLocator DueDate => new(_page, Scope + "DueDate");
        public ILocator Result => _page.Locator("#todo-result");
        public ILocator TitleError => _page.Locator("span[data-valmsg-for='Title']");
        public ILocator DueDateError => _page.Locator("span[data-valmsg-for='DueDate']");
    }
}
