using System.Collections.Concurrent;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridActionCompleteSaveEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ActionCompleteSaveEdit";
    private const string RosterId = "pharmacy-roster";

    private const int ResidentColumn = 2;
    private const int NurseColumn = 4;

    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator Editor(string field) => Page.Locator($"#{RosterId}{field}");
    private ILocator PharmacyNotice => Page.Locator("#pharmacy-notice");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    private async Task Correct(string resident, int column, string field, string current, string corrected)
    {
        await Cell(resident, column).DblClickAsync();
        await Expect(Editor(field)).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await Editor(field).FillAsync(corrected);
        await Editor(field).PressAsync("Enter");
    }

    [Test]
    public async Task a_corrected_name_reaches_the_pharmacy_with_the_old_and_new_name()
    {
        await NavigateAndBoot();

        await Correct("Henry Lui", ResidentColumn, "residentName", "Henry Lui", "Henry Liu");

        await Expect(Cell("Henry Liu", ResidentColumn)).ToHaveTextAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(PharmacyNotice).ToHaveTextAsync(
            "Riverside Pharmacy has the corrected name: Henry Lui is now Henry Liu (Maple roster line 3).",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_saved_change_that_keeps_the_name_sends_the_pharmacy_nothing()
    {
        var notices = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Grid/ActionCompleteSaveEditNotice")) notices.Enqueue(request.Url);
        };
        await NavigateAndBoot();

        await Correct("Henry Lui", NurseColumn, "primaryNurse", "Elena Ruiz", "Owen Park");
        await Expect(Cell("Henry Lui", NurseColumn)).ToHaveTextAsync("Owen Park", new() { Timeout = 10000 });
        await Expect(PharmacyNotice).ToHaveTextAsync("No name corrections sent to the pharmacy yet.");

        await Correct("Grace Benett", ResidentColumn, "residentName", "Grace Benett", "Grace Bennett");

        await Expect(PharmacyNotice).ToHaveTextAsync(
            "Riverside Pharmacy has the corrected name: Grace Benett is now Grace Bennett (Maple roster line 2).",
            new() { Timeout = 10000 });
        Assert.That(notices, Has.Count.EqualTo(1), "Only the name correction is sent to the pharmacy.");
        AssertNoConsoleErrors();
    }
}
