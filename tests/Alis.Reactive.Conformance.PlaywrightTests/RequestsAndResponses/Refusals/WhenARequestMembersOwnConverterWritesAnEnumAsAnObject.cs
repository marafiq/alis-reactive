using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestMembersOwnConverterWritesAnEnumAsAnObject(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_21_0)]
    public async Task the_page_is_refused_where_it_renders_naming_the_member_and_the_object_its_converter_writes()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.RoundsByItsOwnConverter);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Choice' holds Minimum as {\"code\":0} under its own JSON converter, and a request sends one text or number per member; "
                + "send a member written as text or a number.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.RoundsByItsName);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Low", Exact = true }).ClickAsync();
        var sent = await rendered.Page.RunAndWaitForRequestAsync(() => rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find rounds", Exact = true }).ClickAsync(), "**/staffing-rounds**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/refusals/requests-and-responses/staffing-rounds?Choice=Minimum"));
    }
}
