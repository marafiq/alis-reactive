using System.Text.Json;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// As a care coordinator editing a resident's nickname, I can click elsewhere on the page without
/// losing the edit, come back to it, and save it with the card's own button.
/// </summary>
/// <remarks>
/// The nickname card's editor ignores clicks outside it (ActionOnBlur Ignore), so only the card's
/// Save button commits the edit and only its Continue button returns the cursor to it.
/// </remarks>
[TestFixture]
public class WhenCoordinatorFinishesANicknameEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    private ILocator ValueWrapper => Page.Locator("#card-nickname-error .e-editable-value-wrapper").First;
    private ILocator NicknameValue => Page.Locator("#card-nickname-error .e-editable-value");
    private ILocator NicknameInput => Page.Locator("#card-nickname-error input.e-control");

    private async Task StartEditingTheNickname(string nickname)
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-nickname-error .e-editable-value-wrapper");
        await ValueWrapper.ClickAsync();
        await Expect(NicknameInput.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await NicknameInput.First.FillAsync(nickname);
    }

    [Test]
    public async Task saving_from_the_card_commits_the_open_nickname_edit()
    {
        await StartEditingTheNickname("Nell");

        var commit = await Page.RunAndWaitForRequestAsync(
            () => Page.Locator("#nickname-save").ClickAsync(),
            request => request.Method == "POST" && request.Url.Contains("/FusionInPlaceEditor/UpdateNickname"));

        using (var body = JsonDocument.Parse(commit.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.GetProperty("Value").GetString(), Is.EqualTo("Nell"), commit.PostData);
        }

        await Expect(NicknameValue).ToHaveTextAsync("Nell", new() { Timeout = 5000 });
        await Expect(NicknameInput).ToHaveCountAsync(0);
        await Expect(Page.Locator("#card-nickname-display"))
            .ToHaveTextAsync("Nell (resident=resident-42)", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task continuing_the_edit_puts_the_cursor_back_in_the_open_nickname()
    {
        await StartEditingTheNickname("Nel");

        // The click moves keyboard focus to the button; Continue hands it back to the editor.
        await Page.Locator("#nickname-continue").ClickAsync();

        await Expect(NicknameInput.First).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(NicknameInput.First).ToHaveValueAsync("Nel");
        AssertNoConsoleErrors();
    }
}
