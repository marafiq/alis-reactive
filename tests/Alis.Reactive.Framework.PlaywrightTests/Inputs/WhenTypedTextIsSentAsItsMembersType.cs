namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// An entry's text sent in a form post as the type of the member it binds (P4, I1), as the endpoint that binds
/// it says: a day or a moment left empty as the value its type defaults to, a day with a time as that local time
/// to the millisecond, and text that is no day or no number as it was typed, for the server to refuse
/// (grammar.md 2.1: a value that cannot be converted is kept as it is); and the text of a list member's entry
/// as a list, empty when the entry is.
/// </summary>
[TestFixture]
public sealed class WhenTypedTextIsSentAsItsMembersType : AppPageTest
{
    private ILocator Booked => Page.Locator("#booked");

    private async Task Book(string day, string called, string guests)
    {
        await Page.GetByLabel("Day", new() { Exact = true }).FillAsync(day);
        await Page.GetByLabel("Called at", new() { Exact = true }).FillAsync(called);
        await Page.GetByLabel("Guests", new() { Exact = true }).FillAsync(guests);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Book", Exact = true }).ClickAsync();
    }

    private async Task BookAsJson(string tags)
    {
        await Page.GetByLabel("Day", new() { Exact = true }).FillAsync("2026-10-27");
        await Page.GetByLabel("Called at", new() { Exact = true }).FillAsync("2026-10-27T05:30:00Z");
        await Page.GetByLabel("Guests", new() { Exact = true }).FillAsync("2");
        await Page.GetByLabel("Tags", new() { Exact = true }).FillAsync(tags);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Book as JSON", Exact = true }).ClickAsync();
    }

    [Test]
    public async Task a_day_left_empty_is_sent_as_the_day_its_type_defaults_to()
    {
        await Open("/Inputs/Booking");

        await Book("", "2026-10-27T05:30:00Z", "2");

        await Expect(Booked).ToHaveTextAsync("day 0001-01-01 00:00:00.000, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task a_moment_left_empty_is_sent_as_the_moment_its_type_defaults_to()
    {
        await Open("/Inputs/Booking");

        await Book("2026-10-27", "", "2");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 00:00:00.000, called 0001-01-01 00:00:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task a_day_typed_with_a_time_is_sent_as_that_time_of_that_day()
    {
        await Open("/Inputs/Booking");

        await Book("2026-10-27T14:30", "2026-10-27T05:30:00Z", "2");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 14:30:00.000, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task a_time_typed_to_the_millisecond_is_sent_to_the_millisecond()
    {
        await Open("/Inputs/Booking");

        await Book("2026-10-27T14:30:15.250", "2026-10-27T05:30:00Z", "2");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 14:30:15.250, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task a_day_not_on_the_calendar_is_sent_as_typed()
    {
        await Open("/Inputs/Booking");

        await Book("2026-02-30", "2026-10-27T05:30:00Z", "2");

        await Expect(Booked).ToHaveTextAsync("day '2026-02-30' not bound, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task text_that_is_no_day_is_sent_as_typed()
    {
        await Open("/Inputs/Booking");

        await Book("next Tuesday", "2026-10-27T05:30:00Z", "2");

        await Expect(Booked).ToHaveTextAsync("day 'next Tuesday' not bound, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task text_that_is_no_number_is_sent_as_typed()
    {
        await Open("/Inputs/Booking");

        await Book("2026-10-27", "2026-10-27T05:30:00Z", "two");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 00:00:00.000, called 2026-10-27 05:30:00 +00:00, guests 'two' not bound, no tags");
    }

    [Test]
    public async Task a_list_member_whose_entry_is_left_empty_is_sent_as_an_empty_list()
    {
        await Open("/Inputs/Booking");

        await BookAsJson("");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 00:00:00.000, called 2026-10-27 05:30:00 +00:00, guests 2, no tags");
    }

    [Test]
    public async Task a_word_in_the_entry_of_a_list_member_is_sent_as_a_list_of_that_word()
    {
        await Open("/Inputs/Booking");

        await BookAsJson("garden");

        await Expect(Booked).ToHaveTextAsync("day 2026-10-27 00:00:00.000, called 2026-10-27 05:30:00 +00:00, guests 2, tags 'garden'");
    }
}
