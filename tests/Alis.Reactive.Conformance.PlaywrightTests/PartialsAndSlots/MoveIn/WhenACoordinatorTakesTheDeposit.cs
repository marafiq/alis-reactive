using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorTakesTheDeposit(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X165_0)]
    public async Task the_accepted_quote_takes_one_deposit_and_reopens_its_paid_receipt()
    {
        var unaccepted = await NewWorld();
        await unaccepted.Page.GotoAsync(CarePages.MoveIn);
        await Expect(unaccepted.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
        await unaccepted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await unaccepted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4111111111111111");
        await unaccepted.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        await unaccepted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        await Expect(unaccepted.Page.Locator("#deposit-status")).ToHaveTextAsync("Accept the current quote before paying.");
        await unaccepted.Page.GotoAsync(CarePages.MoveIn);
        await Expect(unaccepted.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 500.00 USD due.");

        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 500.00 USD due.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4111111111111111");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit authorised: 500.00 USD, card ending 1111");
        await Expect(world.Page.Locator("#deposit-receipt")).ToHaveTextAsync("PAY100");
        await Expect(world.Page.Locator("#deposit-balance")).ToHaveTextAsync("0.00 USD due");
        await Expect(world.Page.Locator("#deposit-readiness")).ToHaveTextAsync("Deposit satisfied");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("1 authorised payment. PAY100: 500.00 USD, card ending 1111. 0.00 USD due.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-receipt")).ToHaveTextAsync("PAY100");
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit authorised: 500.00 USD, card ending 1111");
        await Expect(world.Page.Locator("#deposit-balance")).ToHaveTextAsync("0.00 USD due");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X166_0)]
    public async Task an_invalid_checksum_stops_the_payment_before_it_leaves_the_browser()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4111111111111112");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Deposit", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Enter a valid card number." });
        Assert.That(world.SentTo(CareEndpoints.MoveInDeposits), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 500.00 USD due.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("No authorised receipt for this operation.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X167_0)]
    public async Task the_issuer_declines_its_example_card_and_no_receipt_reopens()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4000000000000002");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("The deposit was declined.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 500.00 USD due.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("No authorised receipt for this operation.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X168_0)]
    public async Task an_uncertain_charge_is_recovered_and_replaying_the_operation_keeps_one_receipt()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInDeposits));
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4111111111111111");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit outcome unknown");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-receipt")).ToHaveTextAsync("PAY100");
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit authorised: 500.00 USD, card ending 1111");
        await world.Seed(MoveInChanges.TheDepositConnectionIsRestored());
        var replay = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInDeposits);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        var held = await replay.Held();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Authorising deposit");
        await held.Release();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit authorised: 500.00 USD, card ending 1111");
        await Expect(world.Page.Locator("#deposit-receipt")).ToHaveTextAsync("PAY100");
        Assert.That(world.SentTo(CareEndpoints.MoveInDeposits), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("1 authorised payment. PAY100: 500.00 USD, card ending 1111. 0.00 USD due.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("Deposit authorised: 500.00 USD, card ending 1111");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X169_0)]
    public async Task a_change_to_the_required_deposit_before_commit_leaves_the_draft_unpaid()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-status")).ToHaveTextAsync("Quote accepted");
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 500.00 USD due.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Cardholder", Exact = true }).FillAsync("Helen Ward");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Card number", Exact = true }).FillAsync("4111111111111111");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I authorise this deposit", Exact = true }).CheckAsync();
        var payment = await world.Arm(RequestGate.Attempt, CareEndpoints.MoveInDeposits);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Authorise deposit", Exact = true }).ClickAsync();
        var held = await payment.Held();
        await Expect(world.Page.Locator("#deposit-progress")).ToHaveTextAsync("Submitting deposit");
        await world.Seed(MoveInChanges.TheRequiredDepositChanges(650m));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 4: 3040.00 USD");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept assistance terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#acceptance-receipt")).ToHaveTextAsync("ACCEPT-Q4");
        await held.Release();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("The required deposit changed to 650.00 USD.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#offered-deposit")).ToHaveTextAsync("Admission deposit: 650.00 USD");
        await Expect(world.Page.Locator("#deposit-required")).ToHaveTextAsync("650.00 USD required");
        await Expect(world.Page.Locator("#deposit-on-file")).ToHaveTextAsync("No authorised payment. 650.00 USD due.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up deposit", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#deposit-status")).ToHaveTextAsync("No authorised receipt for this operation.");
    }
}
