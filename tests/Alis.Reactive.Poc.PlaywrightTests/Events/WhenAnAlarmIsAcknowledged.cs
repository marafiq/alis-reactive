namespace Alis.Reactive.Poc.PlaywrightTests.Events;

[TestFixture]
public sealed class WhenAnAlarmIsAcknowledged : AppPageTest
{
    [Test]
    public async Task a_write_to_the_alarm_before_the_first_await_reaches_the_monitor_in_its_turn()
    {
        await Open("/Monitors/Acknowledgements");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-alarm")).ToHaveTextAsync("cancelled");
        await Expect(Page.Locator("#ack-status")).ToHaveTextAsync("Acknowledging");
    }

    [Test]
    public async Task a_handler_written_after_one_that_awaits_writes_its_own_alarm_before_its_await()
    {
        await Open("/Monitors/Acknowledgements");

        await Page.Locator("#second-monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#second-monitor .monitor-alarm")).ToHaveTextAsync("cancelled");
    }

    [Test]
    public async Task an_alarm_whose_handler_writes_no_cancel_sounds()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-alarm")).ToHaveTextAsync("sounding");
    }

    [Test]
    public async Task after_an_await_a_property_of_the_patient_the_alarm_holds_is_written_on_that_patient()
    {
        await Open("/Monitors/Acknowledgements");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();
        await Expect(Page.Locator("#monitor .monitor-patient")).ToHaveTextAsync("resting");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Confirm the acknowledgement" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-patient")).ToHaveTextAsync("acknowledged");
    }

    [Test]
    public async Task after_an_await_a_method_the_alarm_hands_over_runs()
    {
        await Open("/Monitors/Acknowledgements");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();
        await Expect(Page.Locator("#monitor .monitor-silenced")).ToHaveTextAsync("not silenced");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Confirm the acknowledgement" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-silenced")).ToHaveTextAsync("silenced");
    }

    [Test]
    public async Task after_an_await_a_read_of_the_alarm_reads_it_as_the_monitor_left_it()
    {
        await Open("/Monitors/Acknowledgements");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();
        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Confirm the acknowledgement" }).ClickAsync();

        await Expect(Page.Locator("#ack-bed")).ToHaveTextAsync("4, moved to 6");
        await Expect(Page.Locator("#ack-status")).ToHaveTextAsync("Acknowledged");
    }

    [Test]
    public async Task a_write_to_the_alarm_after_an_await_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Monitors/CancelledAfterAnAwait");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
    }
}
