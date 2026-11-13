using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppWritesJsonWithItsOwnSettings(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F2.M21_X3_0)]
    public async Task the_apps_own_json_calls_keep_the_names_the_app_chose()
    {
        var preview = await NewWorld();
        await preview.Page.GotoAsync(CarePages.ResidentPreview);
        await preview.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview", Exact = true }).ClickAsync();

        await Expect(preview.Page.Locator("#preview")).ToHaveTextAsync("{\"ResidentId\":7,\"GivenName\":\"Rose\"}");

        var camelCased = await NewWorld();
        await camelCased.Page.GotoAsync(CarePages.ResidentPreviewInCamelCase);
        await camelCased.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview", Exact = true }).ClickAsync();

        await Expect(camelCased.Page.Locator("#preview")).ToHaveTextAsync("{\"residentId\":7,\"givenName\":\"Rose\"}");
    }
}
