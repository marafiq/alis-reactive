using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheCoordinatorPreparesAResidentReview(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F53.M21_X1301_0)]
    public async Task each_wing_lists_residents_who_are_overdue_or_escalated()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.ReviewWorklist);
        var apply = world.Page.GetByRole(AriaRole.Button, new() { Name = "Apply filter", Exact = true });
        var residents = world.Page.GetByRole(AriaRole.Rowheader);
        await apply.ClickAsync();
        await Expect(residents).ToHaveTextAsync(["Ada Brooks", "Ben Ortiz"]);

        await world.Page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Value", Exact = true }).FillAsync("South");
        await apply.ClickAsync();
        await Expect(residents).ToHaveTextAsync(["Dev Singh", "Eve Moss"]);

        var reasons = world.Page.GetByRole(AriaRole.Group).Last;
        await reasons.GetByText("AND", new() { Exact = true }).ClickAsync();
        await apply.ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new() { Name = "Worklist status", Exact = true }))
            .ToHaveTextAsync("0 residents on the worklist.");
        await reasons.GetByText("OR", new() { Exact = true }).ClickAsync();
        await apply.ClickAsync();
        await Expect(residents).ToHaveTextAsync(["Dev Singh", "Eve Moss"]);
    }

    [Test]
    [FailsFor(Promises.M21_F53.M21_X1302_0)]
    public async Task the_referral_preview_uses_the_complete_rules_in_the_current_draft()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.ReviewWorklist);
        var check = world.Page.GetByRole(AriaRole.Button, new() { Name = "Check referral", Exact = true });
        var decision = world.Page.GetByRole(AriaRole.Status, new() { Name = "Referral eligibility", Exact = true });
        var wing = world.Page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Value", Exact = true });

        await check.ClickAsync();
        await Expect(decision).ToHaveTextAsync("Inez Shaw does not match the complete conditions.");
        await wing.FillAsync("");
        await check.ClickAsync();
        await Expect(decision).ToHaveTextAsync("Inez Shaw matches the complete conditions.");
        await wing.FillAsync("North");
        await check.ClickAsync();
        await Expect(decision).ToHaveTextAsync("Inez Shaw does not match the complete conditions.");
    }

    [Test]
    [FailsFor(Promises.M21_F53.M21_X1303_0)]
    public async Task an_unfinished_filter_preserves_the_current_worklist_until_the_wing_is_complete()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.ReviewWorklist);
        var apply = world.Page.GetByRole(AriaRole.Button, new() { Name = "Apply filter", Exact = true });
        var status = world.Page.GetByRole(AriaRole.Status, new() { Name = "Worklist status", Exact = true });
        var wing = world.Page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Value", Exact = true });
        var residents = world.Page.GetByRole(AriaRole.Rowheader);

        await apply.ClickAsync();
        await Expect(residents).ToHaveTextAsync(["Ada Brooks", "Ben Ortiz"]);
        await wing.FillAsync("");
        await apply.ClickAsync();
        await Expect(status).ToHaveTextAsync("Complete the required filter values.");
        await Expect(residents).ToHaveTextAsync(["Ada Brooks", "Ben Ortiz"]);
        await wing.FillAsync("South");
        await apply.ClickAsync();
        await Expect(status).ToHaveTextAsync("2 residents on the worklist.");
        await Expect(residents).ToHaveTextAsync(["Dev Singh", "Eve Moss"]);
    }
}
