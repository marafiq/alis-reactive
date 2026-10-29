namespace Alis.Reactive.Framework.PlaywrightTests.Bindings;

/// <summary>
/// An awaited method's result, held in a variable and read in more than one part of a pipeline (grammar.md
/// 3.1, M5): each read gets the value the method settles with, wherever the plan places it: in a request's
/// WhileLoading and its Finally, after a checked request, after a chained request, in requests started
/// together, and after a branch that asked for it. The slate finds Ben as its second row, a moment after
/// it is asked; the standby slate is not on the page.
/// </summary>
[TestFixture]
public sealed class WhenAPipelineReadsAnAwaitedResultAgain : AppPageTest
{
    private const string BenFound = "Ben is row 2";

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task while_loading_shows_a_result_its_request_then_sends()
    {
        await Open("/Bindings/Round");

        await Click("Tell the desk where Ben is");

        await Expect(Line("telling")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_finally_reads_a_result_its_request_could_not_send()
    {
        await Open("/Bindings/Round");

        await Click("Tell the desk with the standby slate");

        await Expect(Line("closed")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_checked_request_reads_a_result_the_request_sent()
    {
        await Open("/Bindings/Round");
        await Page.GetByLabel("Room").FillAsync("12");

        await Click("Check the room and tell the desk");

        await Expect(Line("told")).ToHaveTextAsync($"the desk was told {BenFound}");
        await Expect(Line("after-check")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_checked_request_reads_the_result_when_the_check_holds_the_request_back()
    {
        await Open("/Bindings/Round");

        await Click("Check the room and tell the desk");

        await Expect(Page.GetByText("'Room' must not be empty.")).ToBeVisibleAsync();
        await Expect(Line("after-check")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_request_reads_a_result_its_chained_request_sent()
    {
        await Open("/Bindings/Round");

        await Click("Open the round, then tell the desk");

        await Expect(Line("told")).ToHaveTextAsync($"the desk was told {BenFound}");
        await Expect(Line("after-chain")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task requests_started_together_each_send_a_result_they_share()
    {
        await Open("/Bindings/Round");

        await Click("Tell both desks at once");

        await Expect(Line("day-desk")).ToHaveTextAsync($"the day desk was told {BenFound}");
        await Expect(Line("night-desk")).ToHaveTextAsync($"the night desk was told {BenFound}");
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_else_if_asked_for()
    {
        await Open("/Bindings/Round");

        await Click("Find Ben");

        await Expect(Line("branch")).ToHaveTextAsync("Ben is on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_and_asked_for()
    {
        await Open("/Bindings/Round");

        await Click("Find Ben with Ada");

        await Expect(Line("branch")).ToHaveTextAsync("Ada and Ben are on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_or_asked_for()
    {
        await Open("/Bindings/Round");

        await Click("Find Zed or Ben");

        await Expect(Line("branch")).ToHaveTextAsync("Zed or Ben is on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_else_if_asked_for_after_an_arm_that_waited()
    {
        await Open("/Bindings/Round");

        await Click("Look for Zed, then find Ben");

        await Expect(Line("branch")).ToHaveTextAsync("Ben is on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_and_asked_for_after_a_term_that_waited()
    {
        await Open("/Bindings/Round");

        await Click("Look for Ada and Ben");

        await Expect(Line("branch")).ToHaveTextAsync("Ada and Ben are on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }

    [Test]
    public async Task a_step_after_a_branch_reads_a_result_an_or_asked_for_after_a_term_that_waited()
    {
        await Open("/Bindings/Round");

        await Click("Look for Zed or Ben");

        await Expect(Line("branch")).ToHaveTextAsync("Zed or Ben is on the round");
        await Expect(Line("after-branch")).ToHaveTextAsync(BenFound);
    }
}
