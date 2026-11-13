using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenOnlyTheFirstBranchThatHoldsRuns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_4_0)]
    public async Task each_assessment_runs_the_first_branch_whose_typed_pain_holds_and_no_branch_after_it()
    {
        var assessment = await NewWorld();
        await assessment.Page.GotoAsync(CarePages.PainAssessment);
        var pain = assessment.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Pain", Exact = true });
        var assess = assessment.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assess", Exact = true });

        await pain.FillAsync("5");
        await assess.ClickAsync();
        await Expect(assessment.Page.Locator("#pain-plan")).ToHaveTextAsync("Call the doctor");
        await Expect(assessment.Page.Locator("#pain-log")).ToHaveTextAsync("doctor");

        await pain.FillAsync("3");
        await assess.ClickAsync();
        await Expect(assessment.Page.Locator("#pain-plan")).ToHaveTextAsync("Give pain relief");
        await Expect(assessment.Page.Locator("#pain-log")).ToHaveTextAsync("relief");

        await pain.FillAsync("1");
        await assess.ClickAsync();
        await Expect(assessment.Page.Locator("#pain-plan")).ToHaveTextAsync("No action");
        await Expect(assessment.Page.Locator("#pain-log")).ToHaveTextAsync("none");
    }
}
