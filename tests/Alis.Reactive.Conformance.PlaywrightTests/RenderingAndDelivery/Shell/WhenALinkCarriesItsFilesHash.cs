using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenALinkCarriesItsFilesHash(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X1_0)]
    public async Task each_notice_opens_at_the_hash_of_the_bytes_it_holds()
    {
        var shell = await NewWorld();
        await shell.Page.GotoAsync(CarePages.Notices);
        var north = await shell.Page.RunAndWaitForRequestAsync(() => shell.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the north notice", Exact = true }).ClickAsync(), "**/north.txt**");
        Assert.That(north.Url, Does.EndWith("/Content/Shell/north.txt?v=Q_hTu7WShgCYp2FAwXcO4q3Wb0cgRT1k3PIlF44jjlc"));
        await Expect(shell.Page.GetByText("North", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await shell.Page.GoBackAsync();
        var south = await shell.Page.RunAndWaitForRequestAsync(() => shell.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the south notice", Exact = true }).ClickAsync(), "**/south.txt**");
        Assert.That(south.Url, Does.EndWith("/Content/Shell/south.txt?v=DdLa5N2DrjCFzYCsG_wo6NdrpR3f2XLCaf6fmKhcCZo"));
        await Expect(shell.Page.GetByText("South", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
