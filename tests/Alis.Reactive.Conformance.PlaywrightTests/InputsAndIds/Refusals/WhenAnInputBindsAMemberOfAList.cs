using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInputBindsAMemberOfAList(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_15_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_lambda_the_list_and_how_the_json_writes_it()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DoseOrderCountOfItems);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Input 'm => m.Items.Count' binds Count, which List<Item> declares, and the app's JSON options write List<Item> as a list, "
                + "so the input has no body path: bind a member of a class they write as an object.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DoseOrderFirstItemsName);
        await rendered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "First item", Exact = true }).FillAsync("Aspirin");
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("First item: Aspirin");
    }
}
