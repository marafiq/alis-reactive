namespace Alis.Reactive.Poc.PlaywrightTests.Events;

[TestFixture]
public sealed class WhenADrugRoundStarts : AppPageTest
{
    [Test]
    public async Task a_write_only_member_of_the_payload_reaches_the_vendor_in_its_turn()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-note")).ToHaveTextAsync("Note: Ward round");
    }

    [Test]
    public async Task a_callback_the_payload_holds_answers_the_vendor()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-signed")).ToHaveTextAsync("Signed for Insulin");
    }

    [Test]
    public async Task a_method_of_the_payload_gives_its_value()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-due")).ToHaveTextAsync("Due: 3");
    }

    [Test]
    public async Task a_list_a_payload_method_returns_is_counted()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-missed")).ToHaveTextAsync("Missed: 2");
    }

    [Test]
    public async Task an_object_a_payload_method_returns_takes_its_call()
    {
        await Open("/DrugRounds/Round");
        await Expect(Page.Locator(".round-dose").Nth(1)).ToHaveTextAsync("Heparin due");

        await Click("Start the round");

        await Expect(Page.Locator(".round-dose").Nth(1)).ToHaveTextAsync("Heparin given");
    }

    [Test]
    public async Task a_list_the_payload_holds_is_filtered_and_read()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-listed")).ToHaveTextAsync("Listed: Insulin, Heparin, Vitamin D");
    }

    [Test]
    public async Task an_async_method_of_the_payload_gives_its_value_after_the_await()
    {
        await Open("/DrugRounds/Round");

        await Click("Start the round");

        await Expect(Page.Locator(".round-resident")).ToHaveTextAsync("Chart: Ada Byron");
    }

    [Test]
    public async Task an_object_the_payload_holds_takes_a_write_after_an_await()
    {
        await Open("/DrugRounds/Trolley");
        await Expect(Page.Locator(".round-trolley")).ToHaveTextAsync("Trolley: unlabelled");

        await Click("Start the round");

        await Expect(Page.Locator(".round-resident")).ToHaveTextAsync("Chart: Ada Byron");
        await Expect(Page.Locator(".round-trolley")).ToHaveTextAsync("Trolley: Morning");
    }

    [Test]
    public async Task an_object_the_payload_holds_takes_a_callback_after_an_await()
    {
        await Open("/DrugRounds/Trolley");
        await Click("Start the round");
        await Expect(Page.Locator(".round-trolley")).ToHaveTextAsync("Trolley: Morning");

        await Click("Sign the trolley");

        await Expect(Page.Locator(".round-trolley-signed")).ToHaveTextAsync("Trolley signed for Insulin");
    }

    [Test]
    public async Task a_write_only_member_of_the_payload_written_after_an_await_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/DrugRounds/NoteAfterAwait");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
    }

    [Test]
    public async Task a_callback_of_the_payload_set_after_an_await_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/DrugRounds/SignatureAfterAwait");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
