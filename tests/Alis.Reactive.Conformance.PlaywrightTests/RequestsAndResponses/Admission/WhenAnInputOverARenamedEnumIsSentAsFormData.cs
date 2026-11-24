using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInputOverARenamedEnumIsSentAsFormData(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_36_0)]
    public async Task the_enum_is_sent_by_its_csharp_name_and_not_as_its_option_holds_it()
    {
        var twoCarers = await NewWorld();
        await twoCarers.Page.GotoAsync(CarePages.NightCover);
        await twoCarers.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await twoCarers.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Two carers");
        var sent = await twoCarers.Page.RunAndWaitForRequestAsync(() => twoCarers.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync(), "**/night-cover/covers");
        Assert.That(sent.PostData, Is.EqualTo("Cover=TwoCarers&Resident=Ada"));
        await Expect(twoCarers.Page.Locator("#saved-cover")).ToHaveTextAsync("Ada: TwoCarers");

        var oneCarer = await NewWorld();
        await oneCarer.Page.GotoAsync(CarePages.NightCover);
        await oneCarer.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        var kept = await oneCarer.Page.RunAndWaitForRequestAsync(() => oneCarer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync(), "**/night-cover/covers");
        Assert.That(kept.PostData, Is.EqualTo("Cover=OneCarer&Resident=Ada"));
        await Expect(oneCarer.Page.Locator("#saved-cover")).ToHaveTextAsync("Ada: OneCarer");
    }
}
