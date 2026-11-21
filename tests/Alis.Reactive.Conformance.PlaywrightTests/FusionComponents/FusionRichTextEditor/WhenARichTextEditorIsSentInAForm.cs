using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARichTextEditorIsSentInAForm(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F38.M21_X1038_0)]
    public async Task the_body_holds_the_content_once_under_the_members_binding_name()
    {
        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.SummarySendingWritten);
        var sent = await written.Page.RunAndWaitForRequestAsync(() => written.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync(), "**/summaries");
        Assert.That(sent.PostData, Is.EqualTo("Summary=%3Cp%3EAte+a+full+lunch.%3C%2Fp%3E"));
        await Expect(written.Page.Locator("#sent")).ToHaveTextAsync("<p>Ate a full lunch.</p>");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.SummarySendingBlank);
        var sentBlank = await blank.Page.RunAndWaitForRequestAsync(() => blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync(), "**/summaries");
        Assert.That(sentBlank.PostData, Is.EqualTo("Summary="));
        await Expect(blank.Page.Locator("#sent")).ToHaveTextAsync("No summary");
    }
}
