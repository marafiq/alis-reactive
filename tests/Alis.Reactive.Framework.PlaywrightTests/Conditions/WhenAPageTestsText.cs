namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// Text an entry holds, compared and tested: equal and not equal, contains, starts with, ends with, a
/// pattern, a minimum length, empty and not empty, and membership of a fixed set.
/// </summary>
[TestFixture]
public sealed class WhenAPageTestsText : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private async Task Check(string name)
    {
        await Page.GetByLabel("Name").FillAsync(name);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Check", Exact = true }).ClickAsync();
    }

    private async Task CheckRoom(string room)
    {
        await Page.GetByLabel("Room").FillAsync(room);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Check", Exact = true }).ClickAsync();
    }

    [Test]
    public async Task equal_text_holds_only_for_the_same_text()
    {
        await Open("/Conditions/Names");

        await Check("Ada");
        await Expect(Line("is-ada")).ToHaveTextAsync("the name is Ada");

        await Check("Adam");
        await Expect(Line("is-ada")).ToHaveTextAsync("the name is not Ada");
    }

    [Test]
    public async Task not_equal_text_holds_for_any_other_text()
    {
        await Open("/Conditions/Names");

        await Check("Adam");
        await Expect(Line("not-ada")).ToHaveTextAsync("someone other than Ada");

        await Check("Ada");
        await Expect(Line("not-ada")).ToHaveTextAsync("Ada herself");
    }

    [Test]
    public async Task contains_holds_for_text_that_holds_the_part_anywhere()
    {
        await Open("/Conditions/Names");

        await Check("Mary Ann Lee");
        await Expect(Line("names-ann")).ToHaveTextAsync("names an Ann");

        await Check("Mary Lee");
        await Expect(Line("names-ann")).ToHaveTextAsync("names no Ann");
    }

    [Test]
    public async Task starts_with_holds_only_for_text_that_begins_with_the_prefix()
    {
        await Open("/Conditions/Names");

        await Check("Dr Ada Byron");
        await Expect(Line("doctor")).ToHaveTextAsync("a doctor");

        await Check("Ada Byron Dr");
        await Expect(Line("doctor")).ToHaveTextAsync("not a doctor");
    }

    [Test]
    public async Task ends_with_holds_only_for_text_that_ends_with_the_suffix()
    {
        await Open("/Conditions/Names");

        await Check("Ben Jackson");
        await Expect(Line("jackson")).ToHaveTextAsync("a Jackson");

        await Check("Jackson Lee");
        await Expect(Line("jackson")).ToHaveTextAsync("not a Jackson");
    }

    [Test]
    public async Task matches_holds_for_text_the_pattern_matches()
    {
        await Open("/Conditions/Names");

        await CheckRoom("B12");
        await Expect(Line("room-code")).ToHaveTextAsync("a room code");

        await CheckRoom("B123");
        await Expect(Line("room-code")).ToHaveTextAsync("not a room code");
    }

    [Test]
    public async Task min_length_holds_for_text_at_least_that_long()
    {
        await Open("/Conditions/Names");

        await Check("Ada");
        await Expect(Line("long-enough")).ToHaveTextAsync("long enough");

        await Check("Al");
        await Expect(Line("long-enough")).ToHaveTextAsync("too short");
    }

    [Test]
    public async Task not_empty_holds_once_any_text_is_given()
    {
        await Open("/Conditions/Names");

        await Check("");
        await Expect(Line("named")).ToHaveTextAsync("no name given");

        await Check("A");
        await Expect(Line("named")).ToHaveTextAsync("named");
    }

    [Test]
    public async Task is_empty_holds_for_empty_text()
    {
        await Open("/Conditions/Names");

        await Check("");
        await Expect(Line("unnamed")).ToHaveTextAsync("no name yet");

        await Check("A");
        await Expect(Line("unnamed")).ToHaveTextAsync("has a name");
    }

    [Test]
    public async Task text_of_only_spaces_is_not_empty_since_nothing_is_trimmed()
    {
        await Open("/Conditions/Names");

        await Check("");
        await Expect(Line("unnamed")).ToHaveTextAsync("no name yet");

        await Check("   ");
        await Expect(Line("unnamed")).ToHaveTextAsync("has a name");
    }

    [Test]
    public async Task in_holds_for_a_value_of_the_set()
    {
        await Open("/Conditions/Names");

        await Check("Ben");
        await Expect(Line("on-round")).ToHaveTextAsync("on the round");

        await Check("Cy");
        await Expect(Line("on-round")).ToHaveTextAsync("not on the round");
    }

    [Test]
    public async Task not_in_holds_for_a_value_outside_the_set()
    {
        await Open("/Conditions/Names");

        await Check("Cy");
        await Expect(Line("off-round")).ToHaveTextAsync("off the round");

        await Check("Ada");
        await Expect(Line("off-round")).ToHaveTextAsync("not off the round");
    }
}
