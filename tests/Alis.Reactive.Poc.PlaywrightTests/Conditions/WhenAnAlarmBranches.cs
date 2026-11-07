namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAnAlarmBranches : AppPageTest
{
    [Test]
    public async Task a_write_to_the_alarm_in_an_else_reaches_the_monitor_when_its_sibling_branch_awaits()
    {
        await Open("/Monitors/AwaitedInOneBranch");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an unnoted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-alarm")).ToHaveTextAsync("cancelled");
    }

    [Test]
    public async Task an_alarm_whose_taken_branch_awaits_and_writes_no_cancel_sounds()
    {
        await Open("/Monitors/AwaitedInOneBranch");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-alarm")).ToHaveTextAsync("sounding");
    }

    [Test]
    public async Task a_write_to_the_alarm_after_a_branch_that_awaits_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Monitors/CancelledAfterABranchThatAwaits");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
    }

    [Test]
    public async Task a_write_to_the_alarm_in_an_else_after_an_else_if_reaches_the_monitor()
    {
        await Open("/Monitors/CancelledAfterAnElseIf");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an unnoted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-alarm")).ToHaveTextAsync("cancelled");
    }

    [Test]
    public async Task a_write_to_the_alarm_in_an_else_after_an_else_if_that_awaits_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Monitors/CancelledAfterAnElseIfThatAwaits");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
    }
}
