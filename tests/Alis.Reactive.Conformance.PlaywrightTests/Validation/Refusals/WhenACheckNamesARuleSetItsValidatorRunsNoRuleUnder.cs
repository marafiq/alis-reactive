using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACheckNamesARuleSetItsValidatorRunsNoRuleUnder(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_49_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_validator_and_the_set()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ShiftRosterApprovedWithNoRowRule);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: RosterRules runs no rule under the rule set 'Approval': declare the rules it checks there inside "
                + "RuleSet(\"Approval\", ..), a rule that holds a child validator among them.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(CarePages.ShiftRoster);
        await Expect(rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Approve all", Exact = true })).ToBeVisibleAsync();
    }
}
