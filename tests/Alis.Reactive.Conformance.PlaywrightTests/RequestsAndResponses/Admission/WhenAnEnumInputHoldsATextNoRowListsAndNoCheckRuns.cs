using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumInputHoldsATextNoRowListsAndNoCheckRuns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_37_0)]
    public async Task the_members_start_is_sent_by_its_csharp_name()
    {
        var agency = await NewWorld();
        await agency.Page.GotoAsync(CarePages.QuickCover);
        await agency.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await agency.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Agency cover");
        var sent = await agency.Page.RunAndWaitForRequestAsync(() => agency.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync(), "**/quick-cover/covers");
        Assert.That(sent.PostData, Is.EqualTo("Cover=TwoCarers&Resident=Ada"));
        await Expect(agency.Page.Locator("#saved-cover")).ToHaveTextAsync("Ada: TwoCarers");

        var oneCarer = await NewWorld();
        await oneCarer.Page.GotoAsync(CarePages.QuickCover);
        await oneCarer.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await oneCarer.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("One carer");
        var listed = await oneCarer.Page.RunAndWaitForRequestAsync(() => oneCarer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync(), "**/quick-cover/covers");
        Assert.That(listed.PostData, Is.EqualTo("Cover=OneCarer&Resident=Ada"));
        await Expect(oneCarer.Page.Locator("#saved-cover")).ToHaveTextAsync("Ada: OneCarer");
    }
}
