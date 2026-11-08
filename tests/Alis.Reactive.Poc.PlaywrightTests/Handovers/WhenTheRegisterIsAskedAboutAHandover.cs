using System.Text.RegularExpressions;
using System.Web;

namespace Alis.Reactive.Poc.PlaywrightTests.Handovers;

[TestFixture]
public sealed class WhenTheRegisterIsAskedAboutAHandover : AppPageTest
{
    private static readonly Regex Register = new("/Handovers/Register");

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "America/Los_Angeles";
        return options;
    }

    [Test]
    public async Task a_dose_is_sent_with_every_place_it_holds()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Dose"), Is.EqualTo("0.125"));
    }

    [Test]
    public async Task a_dose_too_small_for_plain_notation_is_sent_in_plain_digits()
    {
        await Open("/Handovers/Ask");
        await Press("Dilute the dose");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Dose"), Is.EqualTo("0.0000001"));
    }

    [Test]
    public async Task a_fraction_is_sent_as_the_shortest_text_that_reads_back_the_same()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Rate"), Is.EqualTo("0.30000000000000004"));
    }

    [Test]
    public async Task a_yes_or_no_is_sent_as_true_or_false()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Due"), Is.EqualTo("true"));
    }

    [Test]
    public async Task a_day_at_midnight_is_sent_as_the_day_alone()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "PlannedFor"), Is.EqualTo("2026-11-09"));
    }

    [Test]
    public async Task a_day_with_a_time_is_sent_with_its_time_and_no_offset()
    {
        await Open("/Handovers/Ask");
        await Press("Plan for 14:30");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "PlannedFor"), Is.EqualTo("2026-11-09T14:30:00"));
    }

    [Test]
    public async Task a_moment_is_sent_as_the_browsers_clock_with_its_offset()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "StartsAt"), Is.EqualTo("2026-11-09T07:00:00-08:00"));
    }

    [Test]
    public async Task a_moment_on_a_summer_day_is_sent_with_that_days_offset()
    {
        await Open("/Handovers/Ask");
        await Press("Move to the summer");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "StartsAt"), Is.EqualTo("2026-07-09T07:00:00-07:00"));
    }

    [Test]
    public async Task a_list_is_sent_as_one_field_per_item()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(SentEach(asked, "Beds"), Is.EqualTo(new[] { "7", "9" }));
    }

    [Test]
    public async Task an_item_of_a_list_of_optional_numbers_that_reads_nothing_is_sent_empty()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(SentEach(asked, "Waiting"), Is.EqualTo(new[] { "4", "", "6" }));
    }

    [Test]
    public async Task an_item_of_a_list_of_text_that_reads_nothing_is_sent_empty()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(SentEach(asked, "Leads"), Is.EqualTo(new[] { "Ana", "", "Ben" }));
    }

    [Test]
    public async Task a_shift_the_board_holds_is_sent_by_its_name()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Shift"), Is.EqualTo("Night"));
    }

    [Test]
    public async Task a_shift_the_view_names_is_sent_by_its_name()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Cover"), Is.EqualTo("Twilight"));
    }

    [Test]
    public async Task an_optional_member_set_from_a_whole_number_is_sent_as_that_number()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(Sent(asked, "Target"), Is.EqualTo("3"));
    }

    [Test]
    public async Task a_number_read_into_a_header_is_sent_as_its_invariant_text()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(await asked.HeaderValueAsync("X-Round"), Is.EqualTo("3"));
    }

    [Test]
    public async Task a_header_that_reads_nothing_is_not_sent()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(await asked.HeaderValueAsync("X-Lead"), Is.Null);
    }

    [Test]
    public async Task a_header_read_from_a_handover_that_started_is_sent()
    {
        await Open("/Handovers/Ask");
        await Press("Start the handover");

        var asked = await Page.RunAndWaitForRequestAsync(AskTheRegister, Register);

        Assert.That(await asked.HeaderValueAsync("X-Lead"), Is.EqualTo("Ana"));
    }

    [Test]
    public async Task a_number_a_yes_or_no_and_a_shift_fill_the_path()
    {
        await Open("/Handovers/Ask");

        var opened = await Page.RunAndWaitForRequestAsync(() => Press("Open the round"), new Regex("/Handovers/Round/"));

        Assert.That(opened.Url, Is.EqualTo(BaseUrl + "/Handovers/Round/3/true/Night"));
    }

    private Task AskTheRegister() => Press("Ask the register");

    private Task Press(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private static string Sent(IRequest request, string key) => HttpUtility.ParseQueryString(new Uri(request.Url).Query)[key] ?? "not sent";

    private static string[] SentEach(IRequest request, string key) => HttpUtility.ParseQueryString(new Uri(request.Url).Query).GetValues(key) ?? [];
}
