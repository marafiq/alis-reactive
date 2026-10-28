namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// <c>Select</c> of a member, of a nested member, and of each text method the lambda compiler maps to its
/// JavaScript form (<c>Trim</c>, <c>TrimStart</c>, <c>TrimEnd</c>, <c>ToUpper</c>, <c>ToUpperInvariant</c>,
/// <c>ToLower</c>, <c>ToLowerInvariant</c>), of those methods chained, of <c>??</c> and <c>?:</c>, of a test
/// an operator or a method makes, and of a date's month where an item has no date: one value per item, in the
/// items' order. The residents endpoint answers Cyd Marsh (81, present, room "  n12 ",
/// keyworker Fay, nickname blank), Ada Hartley (102, away, "E4  ", Gus, no nickname, left on 30 April 2026), Ben
/// Hart (79, present, " n7", Fay, Benny) and Dot Ashton (80, away, "S2", Hal, Dottie), in that order; only Ada
/// has left. A page's text collapses runs of spaces, as a browser shows them, and shows a list's missing value
/// as nothing between its commas.
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

    [Test]
    public async Task a_chain_of_text_methods_applies_each_in_the_order_written()
    {
        await Open("/Arrays/Projections");

        await Button("Rooms trimmed, in capitals").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("N12,E4,N7,S2");
    }

    [Test]
    public async Task coalesce_gives_the_first_value_that_is_not_missing_and_keeps_a_blank_one()
    {
        await Open("/Arrays/Projections");

        await Button("Nicknames, or names where there is none").ClickAsync();

        await Expect(Projected).ToHaveTextAsync(",Ada Hartley,Benny,Dottie");
    }

    [Test]
    public async Task a_conditional_gives_the_branch_its_test_chooses()
    {
        await Open("/Arrays/Projections");

        await Button("Here or away").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("here,away,here,away");
    }

    [Test]
    public async Task a_test_projected_is_each_items_answer()
    {
        await Open("/Arrays/Projections");

        await Button("Whether each is older than 80").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("true,true,false,false");
    }

    [Test]
    public async Task a_test_a_method_makes_projected_is_each_items_answer()
    {
        await Open("/Arrays/Projections");

        await Button("Whether each goes by no nickname").ClickAsync();

        await Expect(Projected).ToHaveTextAsync("true,true,false,false");
    }

    [Test]
    public async Task a_month_of_a_date_an_item_does_not_have_is_missing_and_not_a_number()
    {
        await Open("/Arrays/Projections");

        await Button("The month each left the home").ClickAsync();

        await Expect(Projected).ToHaveTextAsync(",4,,");
    }
}
