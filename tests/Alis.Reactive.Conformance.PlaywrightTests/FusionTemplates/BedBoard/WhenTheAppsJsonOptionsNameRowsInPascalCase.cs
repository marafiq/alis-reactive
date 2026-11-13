using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.PascalCase))]
[Category(Hosts.PascalCaseName)]
public sealed class WhenTheAppsJsonOptionsNameRowsInPascalCase(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F1.X15_5_0)]
    public async Task the_name_cells_read_the_names_the_server_sent()
    {
        var pascalCase = await NewWorld();
        await pascalCase.Page.GotoAsync(CarePages.ResidentRoster);
        await Expect(pascalCase.Page.Locator("#roster-status")).ToHaveTextAsync("Loaded");

        await Expect(pascalCase.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Bo" });

        var camelCase = await NewWorldUnder(Hosts.Net10Name);
        await camelCase.Page.GotoAsync(CarePages.ResidentRoster);
        await Expect(camelCase.Page.Locator("#roster-status")).ToHaveTextAsync("Loaded");

        await Expect(camelCase.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Bo" });
    }
}
