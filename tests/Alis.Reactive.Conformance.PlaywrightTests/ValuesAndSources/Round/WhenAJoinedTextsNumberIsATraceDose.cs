using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJoinedTextsNumberIsATraceDose(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_18_0)]
    public async Task the_number_shows_as_the_server_writes_it()
    {
        var trace = await NewWorld();
        await trace.Page.GotoAsync(CarePages.DoseLookupOfATraceDose);
        await trace.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the dose", Exact = true }).ClickAsync();
        await Expect(trace.Page.Locator("#dose-line")).ToHaveTextAsync("Dose 5E-05 g");

        var halfGram = await NewWorld();
        await halfGram.Page.GotoAsync(CarePages.DoseLookupOfAHalfGramDose);
        await halfGram.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the dose", Exact = true }).ClickAsync();
        await Expect(halfGram.Page.Locator("#dose-line")).ToHaveTextAsync("Dose 0.5 g");
    }
}
