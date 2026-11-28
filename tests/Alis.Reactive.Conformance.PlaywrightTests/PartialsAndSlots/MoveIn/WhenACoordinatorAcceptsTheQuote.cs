using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorAcceptsTheQuote(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X145_0)]
    public async Task accepting_the_displayed_quote_reads_its_receipt_and_exact_assistance()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        var terms = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInAssistance);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        var termsHeld = await terms.Held();
        await Expect(world.Page.Locator("#acceptance-receipt")).ToHaveTextAsync("ACCEPT-Q3");
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Reading assistance terms");
        await termsHeld.Release();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await Expect(world.Page.Locator("#accepted-assistance")).ToHaveTextAsync("Harbor Mutual contribution: 2432.00");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
        await Expect(world.Page.Locator("#assistance-on-file")).ToHaveTextAsync("Agreed to AST-100 for 2432.00 USD");
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
        await Expect(world.Page.Locator("#assistance-on-file")).ToHaveTextAsync("Agreed to AST-100 for 2432.00 USD");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X146_0)]
    public async Task required_assistance_without_agreement_leaves_the_current_price_unaccepted()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Accept the assistance terms.");
        Assert.That(world.SentTo(CareEndpoints.MoveInAcceptance), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X147_0)]
    public async Task a_price_conflict_requires_a_second_agreement_to_the_returned_revision()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Seed(MoveInChanges.ThePharmacyPriceChanges());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Price changed to 3060.00 USD");
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Accept the assistance terms.");
        Assert.That(world.SentTo(CareEndpoints.MoveInAcceptance), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD, assistance 2448.00 USD. 0 calculations on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-receipt")).ToHaveTextAsync("ACCEPT-Q4");
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 4");
        await Expect(world.Page.Locator("#assistance-on-file")).ToHaveTextAsync("Agreed to AST-100 for 2448.00 USD");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X148_0)]
    public async Task a_lost_acceptance_connection_preserves_the_offer_for_an_explicit_retry()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInAcceptance));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Acceptance unavailable. Retry this quote.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await world.Seed(MoveInChanges.TheAcceptanceConnectionIsRestored());
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X149_0)]
    public async Task a_price_push_during_acceptance_leaves_the_new_revision_unaccepted()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        var acceptance = await world.Arm(RequestGate.Attempt, CareEndpoints.MoveInAcceptance);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        var acceptanceHeld = await acceptance.Held();
        await world.Seed(MoveInChanges.ThePharmacyPriceChanges());
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD");
        await acceptanceHeld.Release();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Price changed to 3060.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD, assistance 2448.00 USD. 0 calculations on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");

        var late = await NewWorld();
        await late.Page.GotoAsync(CarePages.MoveIn);
        await late.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await late.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await late.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await late.Opened(CareEndpoints.MoveInAvailabilityStream);
        await late.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        var completed = await late.Arm(CompositionGate.Answer, CareEndpoints.MoveInAcceptance);
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        var completedHeld = await completed.Held();
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read saved service review", Exact = true }).ClickAsync();
        await Expect(late.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
        var replacement = await late.Arm(CompositionGate.Answer, CareEndpoints.MoveInCurrentOffer);
        await late.Seed(MoveInChanges.ThePharmacyPriceChanges());
        var replacementHeld = await replacement.Held();
        await Expect(late.Page.Locator("#acceptance-status")).ToHaveTextAsync("Checking the current quote.");
        await completedHeld.Release();
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read saved service review", Exact = true }).ClickAsync();
        await Expect(late.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD, assistance 2448.00 USD. 0 calculations on file.");
        await Expect(late.Page.Locator("#acceptance-status")).ToHaveTextAsync("Checking the current quote.");
        Assert.That(late.SentTo(CareEndpoints.MoveInAssistance), Is.EqualTo(0));
        await late.Push(CareEndpoints.MoveInAvailabilityStream, "bedChanged", """{"communityId":"C1","bed":{"id":"BED4","apartment":{"id":"A7","number":"7","building":{"id":"B1","name":"Cedar","community":{"id":"C1","name":"Harbor House"}}},"revision":12,"available":true},"priceRevision":3}""");
        await late.Seed(MoveInChanges.TheBedIsTaken());
        await Expect(late.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        Assert.That(late.SentTo(CareEndpoints.MoveInCurrentOffer), Is.EqualTo(2));
        await replacementHeld.Release();
        await Expect(late.Page.Locator("#acceptance-status")).ToHaveTextAsync("Price changed to 3060.00 USD");
        await Expect(late.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD");
        await Expect(late.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await late.Page.GotoAsync(CarePages.MoveIn);
        await Expect(late.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await Expect(late.Page.Locator("#assistance-on-file")).ToHaveTextAsync("Awaiting agreement to AST-100 for 2448.00 USD");
    }
}
