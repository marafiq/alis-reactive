namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

[TestFixture]
public sealed class WhenAPageAsksAnArrayForOneValue : AppPageTest
{
    private ILocator Told => Page.Locator("#told");

    private ILocator Answer => Page.Locator("#answer");

    private ILocator Found => Page.Locator("#found");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task count_gives_how_many_items_there_are()
    {
        await Open("/Arrays/Questions");

        await Button("How many residents?").ClickAsync();

        await Expect(Told).ToHaveTextAsync("4");
    }

    [Test]
    public async Task count_with_a_test_gives_how_many_items_it_holds_for()
    {
        await Open("/Arrays/Questions");

        await Button("How many are present?").ClickAsync();

        await Expect(Told).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_sum_of_whole_numbers_adds_each_items_number()
    {
        await Open("/Arrays/Questions");

        await Button("Their ages added up").ClickAsync();

        await Expect(Told).ToHaveTextAsync("342");
    }

    [Test]
    public async Task a_sum_of_decimals_adds_each_items_number()
    {
        await Open("/Arrays/Questions");

        await Button("Their balances added up").ClickAsync();

        await Expect(Told).ToHaveTextAsync("50.5");
    }

    [Test]
    public async Task a_sum_of_doubles_adds_each_items_number()
    {
        await Open("/Arrays/Questions");

        await Button("Their weights added up").ClickAsync();

        await Expect(Told).ToHaveTextAsync("269.25");
    }

    [Test]
    public async Task a_sum_of_no_items_is_zero()
    {
        await Open("/Arrays/Questions");

        await Button("The ages of those over 110 added up").ClickAsync();

        await Expect(Told).ToHaveTextAsync("0");
    }

    [Test]
    public async Task any_holds_when_there_are_items_and_not_when_there_are_none()
    {
        await Open("/Arrays/Questions");

        await Button("Is there anyone over 100?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("yes");

        await Button("Is there anyone over 110?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("no");
    }

    [Test]
    public async Task any_with_a_test_holds_when_one_item_passes_and_not_when_none_does()
    {
        await Open("/Arrays/Questions");

        await Button("Is any of them over 100?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("yes");

        await Button("Is any of them over 110?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("no");
    }

    [Test]
    public async Task all_holds_when_every_item_passes_and_not_when_one_fails()
    {
        await Open("/Arrays/Questions");

        await Button("Are they all 79 or older?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("yes");

        await Button("Are they all present?").ClickAsync();
        await Expect(Answer).ToHaveTextAsync("no");
    }

    [Test]
    public async Task find_gives_the_first_item_that_passes_or_nothing_when_none_does()
    {
        await Open("/Arrays/Questions");

        await Button("The first named Hart").ClickAsync();
        await Expect(Found).ToHaveTextAsync("Ada Hartley");

        await Button("The first named Zed").ClickAsync();
        await Expect(Found).ToHaveTextAsync("no one");
    }

    [Test]
    public async Task find_with_a_selector_gives_a_member_of_the_first_item_that_passes_or_nothing_when_none_does()
    {
        await Open("/Arrays/Questions");

        await Button("The name of the first over 80").ClickAsync();
        await Expect(Found).ToHaveTextAsync("Cyd Marsh");

        await Button("The name of the first over 110").ClickAsync();
        await Expect(Found).ToHaveTextAsync("no one");
    }
}
