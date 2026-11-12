using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionOrdersAValueItsOwnNumberHandlingWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_19_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_and_its_number_handling()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DoseOfAStrengthWrittenAsText);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A condition orders 'd.Strength', which its own JSON number handling writes unlike the app's options, "
                + "so the browser does not order it as .NET does; order it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DoseOfAStrengthWrittenAsANumber);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the strength", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#strength")).ToHaveTextAsync("above 5 mg");
    }
}
