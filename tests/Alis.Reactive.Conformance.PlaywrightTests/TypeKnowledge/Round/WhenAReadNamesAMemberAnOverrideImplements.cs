using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TypeKnowledge;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAReadNamesAMemberAnOverrideImplements(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M16_F1.X18_7_0)]
    public async Task the_member_is_read_by_its_declarations_json_name()
    {
        var charge = await NewWorld();
        await charge.Page.GotoAsync(CarePages.RoundStaffWithAChargeNurse);
        await charge.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the renamed member", Exact = true }).ClickAsync();
        await Expect(charge.Page.Locator("#renamed")).ToHaveTextAsync("Charge nurse");

        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.RoundStaffWithAStaffNurse);
        await staff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the renamed member", Exact = true }).ClickAsync();
        await Expect(staff.Page.Locator("#renamed")).ToHaveTextAsync("Staff nurse");
    }
}
