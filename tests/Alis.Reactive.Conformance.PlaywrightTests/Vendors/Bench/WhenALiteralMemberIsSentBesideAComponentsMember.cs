using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiteralMemberIsSentBesideAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_beside_static)]
    public async Task the_body_carries_the_literal_under_the_name_its_endpoint_binds()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpBesideStatic);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/notes");
        Assert.That(sent.PostData, Is.EqualTo("{\"code\":\"a-1\",\"source\":\"conformance\"}"));

        var literalless = await NewWorld();
        await literalless.Page.GotoAsync(BenchPages.HttpBesideStatic);
        var sentWithoutTheLiteral = await literalless.Page.RunAndWaitForRequestAsync(() => literalless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load without the literal", Exact = true }).ClickAsync(), "**/notes");
        Assert.That(sentWithoutTheLiteral.PostData, Is.EqualTo("{\"code\":\"a-1\"}"));
    }
}
