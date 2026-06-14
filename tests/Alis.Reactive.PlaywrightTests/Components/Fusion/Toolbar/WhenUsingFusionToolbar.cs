using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar;

// Journey: a resident manages their account from a command bar. They can request
// maintenance or message their care team (which records the started action), and
// pay their balance (which locks the bar, posts the clicked command, and shows the
// server's confirmation). "Done" unlocks the bar again.
[TestFixture]
public class WhenUsingFusionToolbar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionToolbar";
    private const string ToolbarId = "resident-toolbar";

    private FusionToolbarLocator CommandBar => new(Page, ToolbarId);
    private ILocator AccountStatus => Page.Locator("#account-status");
    private ILocator PaymentConfirmation => Page.Locator("#payment-confirmation");
    private ILocator DoneButton => Page.Locator("#payment-done");

    private async Task OpenAccount()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(CommandBar.Command("pay-balance")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionToolbar builder renders the account command bar with the
    // resident's three actions visible.
    [Test]
    public async Task the_account_command_bar_opens_with_the_residents_actions()
    {
        await OpenAccount();

        await Expect(CommandBar.Root).ToBeVisibleAsync();
        await Expect(CommandBar.Command("pay-balance")).ToHaveTextAsync("Pay balance");
        await Expect(CommandBar.Command("request-maintenance")).ToHaveTextAsync("Request maintenance");
        await Expect(CommandBar.Command("message-care-team")).ToHaveTextAsync("Message care team");

        AssertNoConsoleErrors();
    }

    // INTERACTS — clicking a command fires Clicked through the .Reactive wiring; the
    // FusionToolbarClickedArgs.Item.Text payload names the started action in the status banner.
    [Test]
    public async Task requesting_maintenance_shows_which_action_the_resident_started()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("request-maintenance");

        await Expect(AccountStatus).ToHaveTextAsync("Request maintenance", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // The same Clicked pipeline routes a different command to the same status banner with
    // its own text — proving the typed Item.Text carries each clicked item's own label,
    // not a constant.
    [Test]
    public async Task messaging_the_care_team_shows_that_action_started()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("message-care-team");

        await Expect(AccountStatus).ToHaveTextAsync("Message care team", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Item.Id ROUTES the workflow — the Clicked pipeline branches on args.Item.Id Eq
    // "pay-balance". Paying takes the payment branch (server confirmation panel), not the
    // status branch. If Item.Id stopped carrying, this click would fall to the Else branch
    // and the confirmation would never appear.
    [Test]
    public async Task paying_the_balance_runs_the_payment_workflow_and_shows_the_server_confirmation()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("pay-balance");

        await Expect(PaymentConfirmation)
            .ToHaveTextAsync("Your payment of $248.50 was received. Reference: pay-balance.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // Disable — paying locks the whole command bar (the root gains Syncfusion's e-disabled
    // state); "Done" calls Disable(false) and the lock is gone.
    [Test]
    public async Task paying_locks_the_command_bar_and_done_unlocks_it()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("pay-balance");

        await Expect(CommandBar.DisabledRoot).ToBeVisibleAsync(new() { Timeout = 10000 });

        await DoneButton.ClickAsync();

        await Expect(CommandBar.DisabledRoot).ToHaveCountAsync(0, new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the clicked item's typed payload
    // (Item.Id, Item.Text, Item.Disabled) into the POST body under their declared keys.
    // A trusted toolbar click only ever lands on an enabled item, so Item.Disabled's
    // reachable value is false; its fails-when-broken proof is the POST body (P025).
    // (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task paying_posts_the_clicked_command_payload_to_the_server()
    {
        await OpenAccount();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/FusionToolbar/Confirm") && request.Method == "POST",
            new() { Timeout = 10000 });

        await CommandBar.ClickCommand("pay-balance");

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"commandId\":\"pay-balance\""),
            "the gather must carry the clicked Item.Id under its declared key");
        Assert.That(body, Does.Contain("\"commandText\":\"Pay balance\""),
            "the gather must carry the clicked Item.Text under its declared key");
        Assert.That(body, Does.Contain("\"commandDisabled\":false"),
            "the gather must carry the clicked Item.Disabled under its declared key");

        AssertNoConsoleErrors();
    }
}
