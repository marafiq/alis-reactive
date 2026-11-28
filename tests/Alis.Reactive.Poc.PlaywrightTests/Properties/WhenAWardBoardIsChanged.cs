namespace Alis.Reactive.Poc.PlaywrightTests.Properties;

[TestFixture]
public sealed class WhenAWardBoardIsChanged : AppPageTest
{
    [Test]
    public async Task a_written_title_shows_on_the_board_and_the_step_after_it_runs()
    {
        await Open("/Boards/Ward");

        await Page.GetByLabel("Board title").FillAsync("Night shift");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Rename Ward A" }).ClickAsync();

        await Expect(Page.Locator("#ward-a h2")).ToHaveTextAsync("Night shift");
        await Expect(Page.Locator("#rename-status")).ToHaveTextAsync("Renamed");
    }

    [Test]
    public async Task the_right_code_written_to_a_write_only_member_unlocks_the_board_the_page_reads()
    {
        await Open("/Boards/Ward");

        await Page.GetByLabel("Unlock code").FillAsync("4321");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Unlock Ward A" }).ClickAsync();

        await Expect(Page.Locator("#lock-state")).ToHaveTextAsync("Unlocked");
    }

    [Test]
    public async Task a_wrong_code_leaves_the_board_locked()
    {
        await Open("/Boards/Ward");

        await Page.GetByLabel("Unlock code").FillAsync("1111");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Unlock Ward A" }).ClickAsync();

        await Expect(Page.Locator("#lock-state")).ToHaveTextAsync("Locked");
    }

    [Test]
    public async Task a_declared_member_the_board_lacks_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Boards/Ward");

        await Page.GetByLabel("Board title").FillAsync("Night shift");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Rename Ward B" }).ClickAsync();

        await Expect(Page.Locator("#ward-b h2")).ToHaveTextAsync("Ward B");
        await Expect(Page.Locator("#rename-status")).ToHaveTextAsync("");
    }
}
