using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Handovers;

[TestFixture]
public sealed class WhenAHandoverHoldsWhatItsMemberCannotSend : AppPageTest
{
    private static readonly Regex AnyHandoverRequest = new("/Handovers/(Register|Check)");

    [Test]
    public async Task a_day_member_holding_text_that_names_no_day_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Plan after lunch");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task an_instant_member_holding_text_that_names_no_moment_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Start soon");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_number_member_holding_no_number_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Lose the rate");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_boolean_member_holding_text_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Hand due as text");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_text_member_holding_a_number_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Hand the ward as a number");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_list_member_holding_one_number_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Hand one bed");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task an_item_of_a_list_of_whole_numbers_that_reads_nothing_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Lose a late bed");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_shift_member_holding_a_record_sends_no_request()
    {
        await Open("/Handovers/Ask");
        await Press("Hand the shift as a record");

        var first = await Page.RunAndWaitForRequestAsync(AskThenCheck, AnyHandoverRequest);

        Assert.That(new Uri(first.Url).AbsolutePath, Is.EqualTo("/Handovers/Check"));
    }

    [Test]
    public async Task a_request_that_is_not_sent_runs_no_step_after_it()
    {
        await Open("/Handovers/Ask");
        await Press("Hand due as text");
        await Press("Ask the register");

        await Press("Check the handover");

        await Expect(Page.Locator("#checked-round")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#register-asked")).ToHaveTextAsync("no ward yet");
    }

    private async Task AskThenCheck()
    {
        await Press("Ask the register");
        await Press("Check the handover");
    }

    private Task Press(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
