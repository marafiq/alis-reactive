using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextTestReadsItsOperandFromAComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_TextTestOperand)]
    public async Task the_branch_follows_whether_the_note_starts_with_the_components_value()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceTextTestOperand);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Note matches");

        var rewritten = await NewWorld();
        await rewritten.Page.GotoAsync(BenchPages.SourceTextTestOperand);
        await rewritten.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("a-2: checked");
        await rewritten.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(rewritten.Page.Locator("#shown")).ToHaveTextAsync("No match");
    }
}
