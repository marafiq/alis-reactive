using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACheckListsValueIsWritten(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F4.M24_X4_0)]
    public async Task the_write_checks_exactly_the_boxes_whose_values_it_holds()
    {
        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.VisitSchedule);
        await written.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tue and Wed", Exact = true }).ClickAsync();
        await Expect(written.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Tue", Exact = true })).ToBeCheckedAsync();
        await Expect(written.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Wed", Exact = true })).ToBeCheckedAsync();
        await Expect(written.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true })).Not.ToBeCheckedAsync();

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(CarePages.VisitSchedule);
        await Expect(untouched.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true })).ToBeCheckedAsync();
        await Expect(untouched.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Tue", Exact = true })).Not.ToBeCheckedAsync();
        await Expect(untouched.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Wed", Exact = true })).Not.ToBeCheckedAsync();
    }
}
