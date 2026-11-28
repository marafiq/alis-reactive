using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.AntiforgeryField))]
[Category(Hosts.AntiforgeryFieldName)]
public sealed class WhenReceptionUsesFormTokens(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X261_0)]
    public async Task the_reactive_reception_sends_one_form_token_and_records_the_visitors_receipt()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionReception);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        var sent = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visitor", Exact = true }).ClickAsync(), "**/D100/reception");
        Assert.That(sent.Headers.ContainsKey("requestverificationtoken"), Is.False);
        Assert.That(sent.PostData!.Split('&').Count(field => field.StartsWith("__RequestVerificationToken=", StringComparison.Ordinal)), Is.EqualTo(1));
        await Expect(world.Page.Locator("#visitor-receipt")).ToHaveTextAsync("Alice Ward signed in for Helen Ward at North desk");
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("REC100 · Alice Ward signed in for Helen Ward at North desk · Daughter · Consent pending · 1 badge");
    }
}
