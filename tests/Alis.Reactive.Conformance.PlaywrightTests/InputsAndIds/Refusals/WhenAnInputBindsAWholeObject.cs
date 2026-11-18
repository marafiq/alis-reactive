using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInputBindsAWholeObject(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X8_0)]
    public async Task the_page_is_refused_where_the_field_renders_naming_the_input_its_class_and_the_doors_that_carry_it()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.OutingWholeLocation);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Input 'Location' binds Place, which the app's JSON options write as an object, "
                + "and an input binds a member the browser sends as one text, a list of texts or files: bind an input to each value it holds, "
                + "render its view with Html.PartialFor, or Set it in a JSON body from the component that holds it.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.OutingLocationsLatitude);
        await rendered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Latitude", Exact = true }).FillAsync("51.5");
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Latitude: 51.5");
    }
}
