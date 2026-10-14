using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.RichTextEditor;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.RichTextEditor;

/// <summary>
/// As the activities director, I want to write the family newsletter's Garden Club paragraph with its
/// formatting, see it as families will, and save it with its word count, so that the newsletter reads the way
/// I wrote it.
/// </summary>
/// <remarks>
/// The editor opens empty; last month's paragraph is "The Garden Club potted herbs for the dining room windows."
/// with "Garden Club" in bold. The preview follows each change the editor commits: when the director leaves it
/// (IsInteracted true), and every three seconds while she keeps typing in it, at the save interval the page sets
/// (IsInteracted false, rich-text-editor.js:2933-2941, 3301-3304). The server counts the words of the paragraph
/// it is sent.
/// </remarks>
[TestFixture]
public class WhenRichTextEdited : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/RichTextEditor";

    private PagePlan<RichTextEditorModel> _plan = null!;

    private RichTextEditorLocator Paragraph => _plan.RichTextEditor(m => m.Paragraph);
    private ILocator StartFromLastMonth => Page.Locator("#start-from-last-month-btn");
    private ILocator SaveParagraph => Page.Locator("#save-paragraph-btn");
    private ILocator Preview => Page.Locator("#newsletter-preview");
    private ILocator DraftStatus => Page.Locator("#draft-status");
    private ILocator SaveStatus => Page.Locator("#save-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RichTextEditorModel>.FromPage(Page);
        await Expect(Paragraph.Editor).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task WriteInTheEditor(string text)
    {
        await Paragraph.Focus();
        await Page.Keyboard.TypeAsync(text);
    }

    [Test]
    public async Task starting_from_last_months_paragraph_puts_it_in_the_editor()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartFromLastMonth);

        await Expect(Paragraph.Editor).ToHaveTextAsync("The Garden Club potted herbs for the dining room windows.", new() { Timeout = 5000 });
        await Expect(Paragraph.Editor.Locator("strong")).ToHaveTextAsync("Garden Club");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_editor_shows_the_paragraph_as_families_will_see_it()
    {
        await NavigateAndBoot();
        await WriteInTheEditor("Tulips will ");
        await Page.Keyboard.PressAsync("ControlOrMeta+b");
        await Page.Keyboard.TypeAsync("bloom");
        await Page.Keyboard.PressAsync("ControlOrMeta+b");
        await Page.Keyboard.TypeAsync(" by the porch.");

        await Paragraph.Blur();

        await Expect(Preview).ToHaveTextAsync("Tulips will bloom by the porch.", new() { Timeout = 5000 });
        await Expect(Preview.Locator("strong, b")).ToHaveTextAsync("bloom");
        await Expect(DraftStatus).ToHaveTextAsync("Preview updated when you left the editor.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pausing_while_typing_updates_the_preview_without_leaving_the_editor()
    {
        await NavigateAndBoot();

        await WriteInTheEditor("Tulips will bloom by the porch.");

        await Expect(DraftStatus).ToHaveTextAsync("Preview updated while you typed.", new() { Timeout = 10000 });
        await Expect(Preview).ToHaveTextAsync("Tulips will bloom by the porch.");
        await Expect(Paragraph.Editor).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_an_empty_paragraph_is_refused_and_puts_the_cursor_in_the_editor()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveParagraph);

        await Expect(SaveStatus).ToHaveTextAsync("Write the Garden Club paragraph before saving.", new() { Timeout = 5000 });
        await Expect(Paragraph.Editor).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_paragraph_counts_the_words_families_will_read()
    {
        await NavigateAndBoot();
        await WriteInTheEditor("The Garden Club planted tulips by the front porch.");

        await ClickWhenStable(SaveParagraph);

        await Expect(SaveStatus).ToHaveTextAsync("Saved the Garden Club paragraph: 9 words.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_last_months_paragraph_as_it_was_saves_all_its_words()
    {
        await NavigateAndBoot();
        await ClickWhenStable(StartFromLastMonth);
        await Expect(Paragraph.Editor).ToHaveTextAsync("The Garden Club potted herbs for the dining room windows.", new() { Timeout = 5000 });

        await ClickWhenStable(SaveParagraph);

        await Expect(SaveStatus).ToHaveTextAsync("Saved the Garden Club paragraph: 10 words.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
