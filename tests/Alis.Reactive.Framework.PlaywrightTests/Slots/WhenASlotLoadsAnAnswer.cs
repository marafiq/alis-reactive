namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

/// <summary>
/// What a slot does with the answer loaded into it (grammar.md 6.5): unloading a slot releases the slots
/// inside it too (S1, S2); a slot looked up when it is no longer on the page fails only the pipeline that
/// loads it (S3); an answer that is not HTML leaves the slot as it was; HTML with no plan takes the place of
/// what the slot held (S1); and a script in the loaded HTML runs, as a vendor's script that builds its widget
/// does (S1, step 4; L3).
/// </summary>
[TestFixture]
public sealed class WhenASlotLoadsAnAnswer : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Board => Page.GetByRole(AriaRole.Heading, new() { Name = "The notice board" });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private async Task OpenTheBoard()
    {
        await Click("Open the board");
        await Expect(Board).ToBeVisibleAsync();
    }

    private async Task PinANote()
    {
        await Click("Pin a note to the board");
        await Expect(Page.GetByText("The hall closes at nine.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task unloading_a_slot_releases_the_slots_inside_it()
    {
        await Open("/Slots/Hall");
        await OpenTheBoard();
        await PinANote();
        await Click("Ring the bell");
        await Expect(Line("note-heard")).ToHaveTextAsync("1");

        await Click("Close the board");
        await Expect(Line("board-slot")).ToBeEmptyAsync();
        await Click("Ring the bell");

        await Expect(Line("page-heard")).ToHaveTextAsync("2");
        await Expect(Line("note-heard")).ToHaveTextAsync("1");
        await Expect(Line("board-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task loading_into_a_slot_no_longer_on_the_page_fails_only_its_own_pipeline()
    {
        await Open("/Slots/Hall");
        await OpenTheBoard();
        await PinANote();
        await Expect(Line("pin-after")).ToHaveTextAsync("1");
        await Click("Close the board");
        await Expect(Line("board-slot")).ToBeEmptyAsync();

        await Click("Pin a note to the board");

        await Expect(Line("pin-tried")).ToHaveTextAsync("2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Line("pin-after")).ToHaveTextAsync("1");
        await Click("Ring the bell");
        await Expect(Line("page-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task an_answer_that_is_not_html_leaves_the_slot_as_it_was()
    {
        await Open("/Slots/Hall");
        await OpenTheBoard();
        await Click("Ring the bell");
        await Expect(Line("board-heard")).ToHaveTextAsync("1");

        await Click("Put an empty answer on the board");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Click("Ring the bell");

        await Expect(Line("board-heard")).ToHaveTextAsync("2");
        await Expect(Board).ToBeVisibleAsync();
    }

    [Test]
    public async Task html_with_no_plan_takes_the_place_of_what_the_slot_held()
    {
        await Open("/Slots/Hall");
        await OpenTheBoard();
        await Click("Ring the bell");
        await Expect(Line("board-heard")).ToHaveTextAsync("1");

        await Click("Put a plain notice on the board");
        await Expect(Page.GetByText("The hall is being cleaned.")).ToBeVisibleAsync();
        await Click("Ring the bell");

        await Expect(Line("page-heard")).ToHaveTextAsync("2");
        await Expect(Line("board-heard")).ToHaveTextAsync("1");
        await Expect(Board).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_script_in_loaded_html_runs_as_a_vendors_script_that_builds_its_widget()
    {
        await Open("/Slots/Hall");
        await OpenTheBoard();

        await PinANote();

        await Expect(Line("note-stamp")).ToHaveTextAsync("stamped by its own script");
    }
}
