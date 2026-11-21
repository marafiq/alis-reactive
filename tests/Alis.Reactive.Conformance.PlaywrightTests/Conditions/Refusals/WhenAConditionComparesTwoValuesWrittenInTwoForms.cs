using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesTwoValuesWrittenInTwoForms(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_21_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_its_own_converter_writes()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.StatusesWrittenInTwoForms);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A condition compares two values, and its own JSON converter writes 'charted.Status' unlike the app's options, "
                + "so the browser does not compare them as .NET does; compare them on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.StatusesWrittenByTheApp);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Compare the statuses", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#statuses")).ToHaveTextAsync("charted as planned");
    }
}
