using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaReadsPastAMemberItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_13_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path_and_the_member_it_reads_past()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.Index);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Count the converted names", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'b.Converted.Name' reads past 'b.Converted', which its own JSON converter writes, "
                + "so the browser holds what the converter writes, not its members; read 'b.Converted' itself, or read past it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var written = await NewWorld();
        await written.Page.GotoAsync(RefusalPages.Index);
        await written.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Count the written names", Exact = true }).ClickAsync();
        await written.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(written.Page.Locator("#converted-count")).ToHaveTextAsync("2");
    }
}
