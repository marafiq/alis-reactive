using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorDiscardsTheDraft(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X225_0)]
    public async Task only_the_current_open_draft_is_discarded_after_confirmation()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true })).ToHaveCountAsync(1);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var worldQuestion = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(worldQuestion.GetByText("Discard Helen Ward’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        var sent = await world.Page.RunAndWaitForRequestAsync(
            () => worldQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync(), "**/move-ins/D100");
        Assert.That(sent.Method, Is.EqualTo("DELETE"));
        Assert.That(sent.PostData, Is.Null);
        Assert.That(await sent.HeaderValueAsync("If-Match"), Is.EqualTo("7"));
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: success: Draft discarded");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true })).ToHaveCountAsync(0);
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true })).ToHaveCountAsync(0);
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#closed-draft")).ToHaveTextAsync("Helen Ward’s draft is Discarded.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var stale = await NewWorld();
        await stale.Page.GotoAsync(CarePages.MoveIn);
        await stale.Seed(MoveInChanges.LuisSavesTheTransportPlan());
        await stale.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var staleQuestion = stale.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(staleQuestion.GetByText("Discard Helen Ward’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await staleQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(stale.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Version 8 was saved by Luis Ortiz. Reopen the draft.");
        await stale.Page.GotoAsync(CarePages.MoveIn);
        await Expect(stale.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(stale.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(stale.Page.Locator("#saved-draft-notes")).ToHaveTextAsync("Saved notes: Luis arranged transport");
        await stale.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        await staleQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(stale.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: success: Draft discarded");
        await stale.Page.GotoAsync(CarePages.MoveIn);
        await Expect(stale.Page.Locator("#closed-draft")).ToHaveTextAsync("Helen Ward’s draft is Discarded.");
        await Expect(stale.Page.Locator("#saved-version")).ToHaveTextAsync("9");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X226_0)]
    public async Task declining_discard_keeps_the_open_draft_and_its_version()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var worldQuestion = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(worldQuestion.GetByText("Discard Helen Ward’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await worldQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: info: Draft kept");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true })).ToHaveCountAsync(1);
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X227_0)]
    public async Task a_submitted_admission_retains_its_bed_and_version_when_discard_is_refused()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveInOfTheSubmittedDraft);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var worldQuestion = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(worldQuestion.GetByText("Discard Omar King’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await worldQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A submitted admission cannot be discarded.");
        Assert.That(world.SentTo(CarePages.MoveInOfTheSubmittedDraft), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveInOfTheSubmittedDraft);
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Submitted");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("9");
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("ADM203 for R203 in BED12. 1 bed allocation. 0 admission history entries.");
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident name", Exact = true })).ToHaveValueAsync("Omar King");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X228_0)]
    public async Task a_lost_discard_request_leaves_the_draft_open_at_its_saved_version()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CarePages.MoveIn));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var worldQuestion = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(worldQuestion.GetByText("Discard Helen Ward’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await worldQuestion.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Draft could not be discarded.");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#saved-draft-notes")).ToHaveTextAsync("Saved notes: Family brings clothing");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X229_0)]
    public async Task replacing_the_draft_actions_closes_the_question_without_discarding()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#draft-actions-status")).ToHaveTextAsync("Helen Ward · Open · version 7");
        await world.Seed(MoveInChanges.LuisSavesTheTransportPlan());
        var refresh = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInDraftActions);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh draft actions", Exact = true }).ClickAsync();
        var held = await refresh.Held();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        var worldQuestion = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(worldQuestion.GetByText("Discard Helen Ward’s draft?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await held.Release();
        await Expect(world.Page.Locator("#draft-actions-status")).ToHaveTextAsync("Helen Ward · Open · version 8");
        await Expect(worldQuestion).ToBeHiddenAsync();
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#saved-draft-notes")).ToHaveTextAsync("Saved notes: Luis arranged transport");
    }
}
