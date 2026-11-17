using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACheckListIsFocused(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F4.M24_X13_0)]
    public async Task the_focus_lands_on_the_first_box_whichever_is_checked()
    {
        var changed = await NewWorld();
        await changed.Page.GotoAsync(CarePages.VisitDaysChange);
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Change days", Exact = true }).ClickAsync();
        await Expect(changed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true })).ToBeFocusedAsync();

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(CarePages.VisitDaysChange);
        await Expect(untouched.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Wed", Exact = true })).ToBeCheckedAsync();
        await Expect(untouched.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true })).Not.ToBeFocusedAsync();
    }
}
