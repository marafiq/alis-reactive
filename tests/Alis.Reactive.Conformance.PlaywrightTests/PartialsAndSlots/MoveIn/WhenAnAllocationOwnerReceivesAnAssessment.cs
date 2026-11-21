using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAllocationOwnerReceivesAnAssessment(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X70_0)]
    public async Task handing_over_one_peer_twice_records_one_mobility_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sound the alarm", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#assessment-capacity")).ToHaveTextAsync("2 places under review");
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        var armed = await world.Arm(LanesGate.Respond, CareEndpoints.MoveInMobilityReviews);
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        var review = await armed.Held();
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        Assert.That(world.SentTo(CareEndpoints.MoveInMobilityReviews), Is.EqualTo(1));
        await review.Release();
        await Expect(world.Page.Locator("#assessment-result")).ToHaveTextAsync("Mobility review accepted: MOB25");
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");

        var current = await NewWorld();
        await current.Page.GotoAsync(CarePages.MoveIn);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(current.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close allocation assessment", Exact = true }).ClickAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        Assert.That(current.SentTo(CareEndpoints.MoveInMobilityReviews), Is.EqualTo(0));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X71_0)]
    public async Task a_ratio_outside_the_range_is_refused_before_a_mobility_review_leaves()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessed ratio", Exact = true }).FillAsync("101");
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The assessment ratio must be between 0 and 100.");
        Assert.That(world.SentTo(CareEndpoints.MoveInMobilityReviews), Is.EqualTo(0));
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await Expect(world.Page.Locator("#assessment-reference")).ToHaveTextAsync("AS-100");

        var current = await NewWorld();
        await current.Page.GotoAsync(CarePages.MoveIn);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(current.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X72_0)]
    public async Task a_superseded_assessment_saves_no_mobility_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Seed(MoveInChanges.TheAssessmentIsSuperseded("AS-101"));
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#assessment-result")).ToHaveTextAsync("Assessment is superseded.");
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await Expect(world.Page.Locator("#assessment-reference")).ToHaveTextAsync("AS-101");

        var current = await NewWorld();
        await current.Page.GotoAsync(CarePages.MoveIn);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(current.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X73_0)]
    public async Task a_dropped_mobility_request_releases_busy_without_recording_a_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInMobilityReviews));
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#assessment-result")).ToHaveTextAsync("The mobility review request failed.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");

        var current = await NewWorld();
        await current.Page.GotoAsync(CarePages.MoveIn);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(current.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await current.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X74_0)]
    public async Task after_a_peer_is_dropped_and_its_host_restarts_only_the_replacement_records_a_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Drop the peer", Exact = true }).ClickAsync();
        await world.Restart("allocation-panel");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close allocation assessment", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise with a new peer", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#assessment-result")).ToHaveTextAsync("Mobility review accepted: MOB25");
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close allocation assessment", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInMobilityReviews), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");

        var filing = await NewWorld();
        await filing.Page.GotoAsync(CarePages.MoveIn);
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(filing.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await filing.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        var armed = await filing.Arm(LanesGate.Respond, CareEndpoints.MoveInMobilityReviews);
        await filing.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(filing.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: assessment-result");
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close allocation assessment", Exact = true }).ClickAsync();
        await Expect(filing.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await held.Release();
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File assessment", Exact = true }).ClickAsync();
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File assessed ratio", Exact = true }).ClickAsync();
        Assert.That(filing.ConsoleErrors, Is.EqualTo(new[] { "The request's container 'assessment' is missing.", "The request's input 'Alis_Reactive_Conformance_App_Care_MobilityRequest__Ratio' for 'Ratio' is missing." }));
        Assert.That(filing.SentTo(CareEndpoints.MoveInMobilityReviews), Is.EqualTo(1));
        Assert.That(filing.SentTo(CareEndpoints.MoveInOwnerHistory), Is.EqualTo(0));
        await Expect(filing.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeHiddenAsync();
        await Expect(filing.Page.Locator("#mobility-history")).ToHaveTextAsync("No mobility reviews.");
        await filing.Page.GotoAsync(CarePages.MoveIn);
        await Expect(filing.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await filing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessed ratio", Exact = true }).FillAsync("46");
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File assessed ratio", Exact = true }).ClickAsync();
        await Expect(filing.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Mobility reviews", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "MOB25: Mobility assessment, ratio 25.", "MOB46: Mobility assessment, ratio 46." });
        await filing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessed ratio", Exact = true }).FillAsync("38");
        await filing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File assessment", Exact = true }).ClickAsync();
        await Expect(filing.Page.Locator("#assessment-filing-result")).ToHaveTextAsync("Mobility review accepted: MOB38");
        await filing.Page.GotoAsync(CarePages.MoveIn);
        await Expect(filing.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Mobility reviews", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "MOB25: Mobility assessment, ratio 25.", "MOB46: Mobility assessment, ratio 46.", "MOB38: Mobility assessment, ratio 38." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X75_0)]
    public async Task a_nested_unsubscribe_that_throws_still_releases_both_owners_and_the_replacement_records_once()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Freeze", Exact = true }).ClickAsync();
        var armed = await world.Arm(LanesGate.Respond, CareEndpoints.MoveInAllocationOwner);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        var replacement = await armed.Held();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await replacement.Release();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeHiddenAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Allocation review", Exact = true }).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#assessment-result")).ToHaveTextAsync("Mobility review accepted: MOB25");
        Assert.That(world.ConsoleErrors, Is.EqualTo(new[] { "Releasing the slot 'allocation-owner' threw, and every owner was released past it: the slot 'payer-owner' threw while removing its listeners (The listen to 'onSearch' on the element 'room-handoff' of the vendor 'conformance-callbacks' was not removed: The object refused the callback property 'onSearch'.); the slot 'allocation-owner' threw while removing its listeners (The listen to 'onRaised' on the element 'room-handoff' of the vendor 'conformance-callbacks' was not removed: The object refused the callback property 'onRaised'.)" }));
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#mobility-history")).ToHaveTextAsync("MOB25: Mobility assessment, ratio 25.");

        var writable = await NewWorld();
        await writable.Page.GotoAsync(CarePages.MoveIn);
        await writable.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await writable.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(writable.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await writable.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(writable.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeHiddenAsync();
        Assert.That(writable.ConsoleErrors, Is.Empty);
    }
}
