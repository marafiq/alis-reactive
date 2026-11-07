namespace Alis.Reactive.Poc.PlaywrightTests.LiveDates;

[TestFixture]
public sealed class WhenAStepWritesADateIntoALiveMember : AppPageTest
{
    [Test]
    public async Task a_date_a_step_passes_to_a_method_reaches_it_as_a_date()
    {
        await Open("/Visits/Planner");

        await Button("Book Saturday 08:00").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("Saturday 08:00");
    }

    [Test]
    public async Task a_date_inside_an_object_a_step_passes_reaches_the_method_as_a_date()
    {
        await Open("/Visits/Planner");

        await Button("Book Helen Park on Monday").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("Helen Park, Monday 14:30");
    }

    [Test]
    public async Task each_date_of_a_list_a_step_passes_reaches_the_method_as_a_date()
    {
        await Open("/Visits/Planner");

        await Button("Book the weekend").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("Saturday 08:00 and Sunday 09:15");
    }

    [Test]
    public async Task an_instant_a_step_passes_reaches_the_method_as_that_moment()
    {
        await Open("/Visits/Planner");

        await Button("Book 13:00 UTC").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("2026-11-07T13:00:00.000Z");
    }

    [Test]
    public async Task a_nullable_date_reaches_the_method_as_a_date_and_its_null_as_null()
    {
        await Open("/Visits/Planner");
        await Button("Reschedule to Friday 11:00").ClickAsync();
        await Expect(Page.Locator("#booked")).ToHaveTextAsync("Friday 11:00");

        await Button("Leave the visit unscheduled").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("unscheduled");
    }

    [Test]
    public async Task a_date_a_step_writes_into_a_property_reaches_it_as_a_date()
    {
        await Open("/Visits/Planner");

        await Button("Make Thursday 09:00 the next visit").ClickAsync();

        await Expect(Page.Locator("#next-day")).ToHaveTextAsync("Thursday 09:00");
    }

    [Test]
    public async Task a_date_an_event_carries_as_text_reaches_a_method_as_a_date()
    {
        await Open("/Visits/Planner");

        await Button("Request a visit").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("Tuesday 10:00");
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
}
