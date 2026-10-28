namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// <c>Select</c> of a member, of a nested member, and of each text method the lambda compiler maps to its
/// JavaScript form (<c>Trim</c>, <c>TrimStart</c>, <c>TrimEnd</c>, <c>ToUpper</c>, <c>ToUpperInvariant</c>,
/// <c>ToLower</c>, <c>ToLowerInvariant</c>): one value per item, in the items' order. The residents endpoint
/// answers Cyd Marsh (room "  n12 ", keyworker Fay), Ada Hartley ("E4  ", Gus), Ben Hart (" n7", Fay) and
/// Dot Ashton ("S2", Hal), in that order. A page's text collapses runs of spaces, as a browser shows them.
/// </summary>
[TestFixture]
public sealed class WhenAPageProjectsAnArray : AppPageTest
{
    private ILocator Projected => Page.Locator("#projected");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_member_of_each_item_is_projected_in_the_items_order()
    {
        await Open("/Arrays/Projections");

        await Button("Names").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task a_member_of_a_member_of_each_item_is_projected()
    {
        await Open("/Arrays/Projections");

        await Button("Keyworkers").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("Fay,Gus,Fay,Hal");
    }

    [Test]
    public async Task trim_takes_the_spaces_off_both_ends_of_each_text()
    {
        await Open("/Arrays/Projections");

        await Button("Rooms trimmed").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("n12,E4,n7,S2");
    }

    [Test]
    public async Task trim_start_takes_the_spaces_off_the_start_of_each_text_only()
    {
        await Open("/Arrays/Projections");

        await Button("Rooms trimmed at the start").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("n12 ,E4 ,n7,S2");
    }

    [Test]
    public async Task trim_end_takes_the_spaces_off_the_end_of_each_text_only()
    {
        await Open("/Arrays/Projections");

        await Button("Rooms trimmed at the end").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("n12,E4, n7,S2");
    }

    [Test]
    public async Task to_upper_gives_each_text_in_capitals()
    {
        await Open("/Arrays/Projections");

        await Button("Names in capitals").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("CYD MARSH,ADA HARTLEY,BEN HART,DOT ASHTON");
    }

    [Test]
    public async Task to_upper_invariant_gives_each_text_in_capitals()
    {
        await Open("/Arrays/Projections");

        await Button("Names in invariant capitals").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("CYD MARSH,ADA HARTLEY,BEN HART,DOT ASHTON");
    }

    [Test]
    public async Task to_lower_gives_each_text_in_small_letters()
    {
        await Open("/Arrays/Projections");

        await Button("Names in small letters").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("cyd marsh,ada hartley,ben hart,dot ashton");
    }

    [Test]
    public async Task to_lower_invariant_gives_each_text_in_small_letters()
    {
        await Open("/Arrays/Projections");

        await Button("Names in invariant small letters").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("cyd marsh,ada hartley,ben hart,dot ashton");
    }
}
