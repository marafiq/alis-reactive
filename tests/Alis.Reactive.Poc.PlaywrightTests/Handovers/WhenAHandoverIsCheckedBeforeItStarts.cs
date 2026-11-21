namespace Alis.Reactive.Poc.PlaywrightTests.Handovers;

[TestFixture]
public sealed class WhenAHandoverIsCheckedBeforeItStarts : AppPageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    [Test]
    public async Task a_round_that_reads_nothing_is_left_out_and_the_register_keeps_its_own_round()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-round")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#checked-problems")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task a_start_that_reads_nothing_is_left_out_and_the_register_keeps_its_own_start()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-starts")).ToHaveTextAsync("2026-11-09 06:00:00 +00:00");
    }

    [Test]
    public async Task an_end_that_reads_nothing_reaches_the_register_as_no_end()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-ends")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task beds_that_read_nothing_are_left_out_and_the_register_keeps_its_own_beds()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-beds")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#checked-problems")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task a_lead_that_reads_nothing_reaches_the_register_as_no_lead()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-lead")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task a_target_that_reads_nothing_reaches_the_register_as_no_target()
    {
        await Open("/Handovers/Ask");

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-target")).ToHaveTextAsync("none");
    }

    [Test]
    public async Task a_started_handover_reaches_the_register_as_it_stands()
    {
        await Open("/Handovers/Ask");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Start the handover", Exact = true }).ClickAsync();

        await CheckTheHandover();

        await Expect(Page.Locator("#checked-round")).ToHaveTextAsync("4");
        await Expect(Page.Locator("#checked-starts")).ToHaveTextAsync("2026-11-09 19:00:00 -08:00");
        await Expect(Page.Locator("#checked-ends")).ToHaveTextAsync("2026-11-10 07:00:00 -08:00");
        await Expect(Page.Locator("#checked-beds")).ToHaveTextAsync("12");
        await Expect(Page.Locator("#checked-lead")).ToHaveTextAsync("Ana");
        await Expect(Page.Locator("#checked-target")).ToHaveTextAsync("1500");
    }

    private Task CheckTheHandover() => Page.GetByRole(AriaRole.Button, new() { Name = "Check the handover", Exact = true }).ClickAsync();
}
