namespace Alis.Reactive.Poc.PlaywrightTests.Events;

[TestFixture]
public sealed class WhenAVitalsMonitorRaisesAnAlarm : AppPageTest
{
    [Test]
    public async Task a_declared_member_of_the_alarm_reads_as_the_alarm_holds_it()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm-bed")).ToHaveTextAsync("4");
        await Expect(Page.Locator("#alarm-status")).ToHaveTextAsync("Handled");
    }

    [Test]
    public async Task a_text_member_of_the_alarm_shows_the_text_it_holds()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm-note")).ToHaveTextAsync("after surgery");
    }

    [Test]
    public async Task a_text_member_of_the_alarm_holding_null_shows_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an unnoted alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm-note")).ToHaveTextAsync("");
        await Expect(Page.Locator("#alarm-status")).ToHaveTextAsync("Handled");
    }

    [Test]
    public async Task a_field_the_alarms_reading_lacks_reads_its_start()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm-unit")).ToHaveTextAsync("bpm");
    }

    [Test]
    public async Task a_property_of_the_patient_the_alarm_holds_is_written_on_that_patient()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-patient")).ToHaveTextAsync("attending");
    }

    [Test]
    public async Task a_method_of_the_patient_the_alarm_holds_runs_on_that_patient()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-paged")).ToHaveTextAsync("paged");
    }

    [Test]
    public async Task a_method_the_alarm_hands_over_runs()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-silenced")).ToHaveTextAsync("silenced");
    }

    [Test]
    public async Task an_alarm_that_lacks_a_declared_member_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#older-monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an alarm" }).ClickAsync();

        await Expect(Page.Locator("#older-monitor .monitor-raised")).ToHaveTextAsync("raised alarm");
        await Expect(Page.Locator("#alarm-status")).ToHaveTextAsync("Reading");
        await Expect(Page.Locator("#alarm-bed")).ToHaveTextAsync("unread");
    }

    [Test]
    public async Task an_event_declared_with_a_payload_raised_with_no_argument_stops_before_its_first_step()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an empty alarm" }).ClickAsync();

        await Expect(Page.Locator("#monitor .monitor-raised")).ToHaveTextAsync("raised alarm with nothing");
        await Expect(Page.Locator("#alarm-status")).ToHaveTextAsync("waiting");
    }

    [Test]
    public async Task an_event_declared_without_a_payload_runs_when_raised_with_no_argument()
    {
        await Open("/Monitors/Alarms");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Silence the monitor" }).ClickAsync();

        await Expect(Page.Locator("#alarm-status")).ToHaveTextAsync("Silenced");
    }
}
