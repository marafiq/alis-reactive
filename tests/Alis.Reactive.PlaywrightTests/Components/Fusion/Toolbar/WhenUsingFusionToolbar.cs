namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar;

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

    [Test]
    public async Task requesting_maintenance_shows_which_action_the_resident_started()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("request-maintenance");

        await Expect(AccountStatus).ToHaveTextAsync("Request maintenance", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task messaging_the_care_team_shows_that_action_started()
    {
        await OpenAccount();

        await CommandBar.ClickCommand("message-care-team");

        await Expect(AccountStatus).ToHaveTextAsync("Message care team", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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

        Assert.That(body, Does.Contain("\"CommandId\":\"pay-balance\""),
            "the gather must carry the clicked Item.Id under its declared key");
        Assert.That(body, Does.Contain("\"CommandText\":\"Pay balance\""),
            "the gather must carry the clicked Item.Text under its declared key");
        Assert.That(body, Does.Contain("\"CommandDisabled\":false"),
            "the gather must carry the clicked Item.Disabled under its declared key");

        AssertNoConsoleErrors();
    }
}
