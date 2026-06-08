namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridResidentEditForm : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ResidentEditForm";

    private async Task NavigateForm()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#edit-form-load-status"))
            .ToHaveTextAsync("loaded edit form rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-template-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task inline_cell_editors_render_typed_select_and_date_templates()
    {
        await NavigateForm();

        await ClickWhenStable(Page.Locator("#inline-tpl-edit"));
        await Expect(Page.Locator("#inline-template-grid .e-editedrow"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // 2-arg Select template: a <select> bound to careLevel populated from a string list.
        var careLevel = Page.Locator("#inline-template-grid select[name='careLevel']");
        await Expect(careLevel).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(careLevel).ToContainTextAsync("Memory Care", new() { Timeout = 10000 });

        // Typed 4-arg Select template: a <select> bound to primaryNurse with text/value selectors.
        var nurse = Page.Locator("#inline-template-grid select[name='primaryNurse']");
        await Expect(nurse).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(nurse).ToContainTextAsync("Nora Ellis", new() { Timeout = 10000 });

        // DateInput template: a native <input type="date"> bound to nextReviewDate.
        await Expect(Page.Locator("#inline-template-grid input[type='date'][name='nextReviewDate']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await Expect(Page.Locator("#inline-tpl-status"))
            .ToHaveTextAsync("inline edit started", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dialog_admission_form_renders_text_number_date_and_select_templates()
    {
        await NavigateForm();

        await ClickWhenStable(Page.Locator("#dialog-tpl-edit"));
        await Expect(Page.Locator("#dialog-template-grid_dialogEdit_wrapper"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // Text field template.
        await Expect(Page.Locator("#dialog-template-grid_dialogEdit_wrapper input[name='residentName']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        // Number field template.
        await Expect(Page.Locator("#dialog-template-grid_dialogEdit_wrapper input[type='number'][name='openTasks']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        // Date field template.
        await Expect(Page.Locator("#dialog-template-grid_dialogEdit_wrapper input[type='date'][name='nextReviewDate']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        // 3-arg Select field template populated from a string list.
        var risk = Page.Locator("#dialog-template-grid_dialogEdit_wrapper select[name='riskLevel']");
        await Expect(risk).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(risk).ToContainTextAsync("Moderate", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
