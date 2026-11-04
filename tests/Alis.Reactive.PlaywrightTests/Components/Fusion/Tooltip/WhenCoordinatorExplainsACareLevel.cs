using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Tooltip;

[TestFixture]
public class WhenCoordinatorExplainsACareLevel : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Tooltip";

    private static readonly Regex Highlighted = new(@"\bbg-accent/10\b");

    private ILocator Explanation => Page.Locator(".e-tooltip-wrap");
    private ILocator MemoryCareTerm => Page.Locator("#memory-care-term");
    private ILocator Explain => Page.Locator("#explain-memory-care");
    private ILocator GotIt => Page.Locator("#explanation-done");

    private NativeCheckBoxLocator HideExplanations =>
        new(Page, IdGenerator.For<TooltipModel>(m => m.HideExplanations));

    [Test]
    public async Task explaining_memory_care_shows_the_explanation_and_highlights_the_term()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#explain-memory-care");

        await Explain.ClickAsync();

        await Expect(Explanation).ToContainTextAsync("24/7 staff, a secure environment", new() { Timeout = 5000 });
        await Expect(MemoryCareTerm).ToHaveClassAsync(Highlighted);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task got_it_is_offered_only_once_the_explanation_has_opened()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#explain-memory-care");
        await Expect(GotIt).ToBeHiddenAsync();

        await Explain.ClickAsync();

        await Expect(Explanation).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(GotIt).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task got_it_closes_the_explanation_and_clears_the_highlight()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#explain-memory-care");
        await Explain.ClickAsync();
        await Expect(Explanation).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(MemoryCareTerm).ToHaveClassAsync(Highlighted);

        await GotIt.ClickAsync();

        await Expect(Explanation).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(MemoryCareTerm).Not.ToHaveClassAsync(Highlighted);
        await Expect(GotIt).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task with_explanations_turned_off_the_explanation_does_not_open()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#explain-memory-care");
        await HideExplanations.Check();

        await Explain.ClickAsync();

        await Expect(Page.Locator("#explanations-off")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#explanations-off")).ToHaveTextAsync("Explanations are turned off on this screen.");
        await Expect(Explanation).ToHaveCountAsync(0);
        await Expect(MemoryCareTerm).Not.ToHaveClassAsync(Highlighted);
        AssertNoConsoleErrors();
    }
}
