namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// <c>OrderBy</c> and <c>OrderByDescending</c> over text, numbers, dates, moments and flags, and orders chained
/// with filters and projections. Every order asked for differs from the order the residents endpoint answers
/// them in: Cyd Marsh (81, admitted 2026-01-05, present, next visit 07:30 at +05:30), Ada Hartley (102,
/// 2026-03-14, away, 08:00 at +09:00), Ben Hart (79, 2026-06-19, present, 21:00 the day before at -04:00) and
/// Dot Ashton (80, 2025-11-02, away, 00:00 at +00:00).
/// </summary>
[TestFixture]
public sealed class WhenAPageOrdersAnArray : AppPageTest
{
    private ILocator Ordered => Page.Locator("#ordered");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task text_orders_from_first_to_last()
    {
        await Open("/Arrays/Orders");

        await Button("By name").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ada Hartley,Ben Hart,Cyd Marsh,Dot Ashton");
    }

    [Test]
    public async Task text_orders_from_last_to_first_when_descending()
    {
        await Open("/Arrays/Orders");

        await Button("By name, last first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Dot Ashton,Cyd Marsh,Ben Hart,Ada Hartley");
    }

    [Test]
    public async Task numbers_order_by_their_value_not_their_digits()
    {
        await Open("/Arrays/Orders");

        await Button("By age, youngest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ben Hart,Dot Ashton,Cyd Marsh,Ada Hartley");
    }

    [Test]
    public async Task numbers_order_from_largest_to_smallest_when_descending()
    {
        await Open("/Arrays/Orders");

        await Button("By age, eldest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ada Hartley,Cyd Marsh,Dot Ashton,Ben Hart");
    }

    [Test]
    public async Task dates_order_from_earliest_to_latest()
    {
        await Open("/Arrays/Orders");

        await Button("By admission, earliest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Dot Ashton,Cyd Marsh,Ada Hartley,Ben Hart");
    }

    [Test]
    public async Task dates_order_from_latest_to_earliest_when_descending()
    {
        await Open("/Arrays/Orders");

        await Button("By admission, latest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ben Hart,Ada Hartley,Cyd Marsh,Dot Ashton");
    }

    [Test]
    public async Task moments_written_at_different_offsets_order_by_when_they_happen()
    {
        await Open("/Arrays/Orders");

        await Button("By next visit, soonest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ada Hartley,Dot Ashton,Ben Hart,Cyd Marsh");
    }

    [Test]
    public async Task a_flag_orders_false_before_true()
    {
        await Open("/Arrays/Orders");

        await Button("Away first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ada Hartley,Dot Ashton,Cyd Marsh,Ben Hart");
    }

    [Test]
    public async Task a_second_order_keeps_the_first_among_items_whose_keys_are_equal()
    {
        await Open("/Arrays/Orders");

        await Button("By name, then away first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ada Hartley,Dot Ashton,Ben Hart,Cyd Marsh");
    }

    [Test]
    public async Task an_order_after_a_filter_orders_only_the_items_kept()
    {
        await Open("/Arrays/Orders");

        await Button("Present, youngest first").ClickAsync();

        await Expect(Ordered).ToHaveTextAsync("Ben Hart,Cyd Marsh");
    }

    [Test]
    public async Task a_projection_after_a_filter_is_ordered_by_its_own_items()
    {
        await Open("/Arrays/Orders");

        await Button("Names of residents 80 or older, last first").ClickAsync();

        await Expect(Page.Locator("#names-last-first")).ToHaveTextAsync("Dot Ashton,Cyd Marsh,Ada Hartley");
    }
}
