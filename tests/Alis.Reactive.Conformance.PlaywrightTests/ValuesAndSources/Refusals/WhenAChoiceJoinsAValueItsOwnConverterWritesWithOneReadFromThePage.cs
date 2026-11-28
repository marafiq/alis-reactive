using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAChoiceJoinsAValueItsOwnConverterWritesWithOneReadFromThePage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_11_1)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_and_its_own_converter()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.BedsideDoseChosenFromTheSelect);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A choice joins 'search.Chart.Status', which its own JSON converter writes unlike the app's options, "
                + "with another value read from the page; choose between it and a value the view writes, or choose on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.BedsideDoseChosenAsGiven);
        await rendered.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Seen at the bedside", Exact = true }).CheckAsync();
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the dose", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#dose-badge")).ToHaveTextAsync("Given");
    }
}
