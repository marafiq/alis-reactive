using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAFilesBytesChange(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X1_1)]
    public async Task the_link_carries_the_hash_of_the_bytes_the_file_holds_when_the_page_renders()
    {
        var changed = await NewWorld();
        await changed.Seed(Seed.TheNotice("North"));
        await changed.Page.GotoAsync(CarePages.TheWorldsNotice);
        var first = await changed.Page.RunAndWaitForRequestAsync(() => changed.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the world's notice", Exact = true }).ClickAsync(), "**/notice.txt**");
        Assert.That(first.Url, Does.EndWith("/notice.txt?v=Q_hTu7WShgCYp2FAwXcO4q3Wb0cgRT1k3PIlF44jjlc"));
        await Expect(changed.Page.GetByText("North", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await changed.Page.GoBackAsync();
        await changed.Seed(Seed.TheNotice("South"));
        await changed.Page.ReloadAsync();
        var second = await changed.Page.RunAndWaitForRequestAsync(() => changed.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the world's notice", Exact = true }).ClickAsync(), "**/notice.txt**");
        Assert.That(second.Url, Does.EndWith("/notice.txt?v=DdLa5N2DrjCFzYCsG_wo6NdrpR3f2XLCaf6fmKhcCZo"));
        await Expect(changed.Page.GetByText("South", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var rewritten = await NewWorld();
        await rewritten.Seed(Seed.TheNotice("North"));
        await rewritten.Page.GotoAsync(CarePages.TheWorldsNotice);
        await rewritten.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the world's notice", Exact = true }).ClickAsync();
        await Expect(rewritten.Page.GetByText("North", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await rewritten.Page.GoBackAsync();
        await rewritten.Seed(Seed.TheNotice("North"));
        await rewritten.Page.ReloadAsync();
        var again = await rewritten.Page.RunAndWaitForRequestAsync(() => rewritten.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the world's notice", Exact = true }).ClickAsync(), "**/notice.txt**");
        Assert.That(again.Url, Does.EndWith("/notice.txt?v=Q_hTu7WShgCYp2FAwXcO4q3Wb0cgRT1k3PIlF44jjlc"));
        await Expect(rewritten.Page.GetByText("North", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
