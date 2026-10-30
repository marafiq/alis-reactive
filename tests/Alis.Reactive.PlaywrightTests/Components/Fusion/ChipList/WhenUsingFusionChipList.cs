using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ChipList;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ChipList;

/// <summary>
/// As the activities coordinator, I want to tag the next outing with the residents' interests, sign up the
/// residents who asked to go, keep the bus to its four seats with a wait list, and set the days the outing
/// repeats, so that the residents who would enjoy it are invited and everyone on the bus can board.
/// </summary>
/// <remarks>
/// The tags are Gardening, Music, Art, Walking and History. The Garden Center suits Gardening and Walking and
/// runs on Thursdays; the Symphony Matinee suits Music and runs on Sundays. Ava Stone, Leah Kim, Nora Gray,
/// Mateo Reed and Walter Chen are on the bus in that order, so Walter Chen waits for a seat; Nora Gray and
/// Mateo Reed use the wheelchair lift. Edith Moreau, Harold Price and Ruth Alvarez asked to go.
/// Not exercised: each selected chip's data <c>Text</c>, as the page reads the selected chips' names from
/// <c>Texts</c>, which carries the same strings.
/// </remarks>
[TestFixture]
public class WhenUsingFusionChipList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ChipList";

    private PagePlan<ChipListModel> _plan = null!;

    private FusionChipListLocator Interests => new(Page, "trip-interests");
    private FusionChipListLocator AskedToGo => new(Page, "trip-asked-to-go");
    private FusionChipListLocator Riders => new(Page, "trip-riders");
    private FusionChipListLocator RepeatDays => new(Page, "trip-repeat-days");
    private NativeDropDownLocator Destination => _plan.NativeDropDown(m => m.Destination);
    private ILocator TagChange => Page.Locator("#tag-change");
    private ILocator Invitees => Page.Locator("#invitees");
    private ILocator SeatTaken => Page.Locator("#seat-taken");
    private ILocator SeatOpen => Page.Locator("#seat-open");
    private ILocator WaitListLeft => Page.Locator("#wait-list-left");
    private ILocator MarkLiftRiders => Page.Locator("#mark-lift-riders-btn");
    private ILocator LiftOutOfService => Page.Locator("#lift-out-of-service-btn");
    private ILocator AddWeekends => Page.Locator("#add-weekends-btn");
    private ILocator PostToCalendar => Page.Locator("#post-outing-btn");
    private ILocator PostStatus => Page.Locator("#post-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ChipListModel>.FromPage(Page);
        await Expect(Riders.Chips).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    private async Task TagMusic()
    {
        await ClickWhenStable(Interests.Chip("Music"));
        await Expect(Invitees).ToHaveTextAsync("Music: invite Leah Kim and Mateo Reed.", new() { Timeout = 5000 });
    }

    private async Task ChooseGardenCenter()
    {
        await Destination.Select("Garden Center");
        await Expect(RepeatDays.SelectedChips).ToHaveTextAsync(new[] { "Thu" }, new() { Timeout = 5000 });
    }

    [Test]
    public async Task tagging_an_interest_says_it_was_added_to_the_trip()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Interests.Chip("Music"));

        await Expect(TagChange).ToHaveTextAsync("Music added to the trip's tags.", new() { Timeout = 5000 });
        await Expect(Interests.SelectedChips).ToHaveTextAsync(new[] { "Music" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task untagging_an_interest_says_it_came_off_the_trip_and_its_residents_are_no_longer_invited()
    {
        await NavigateAndBoot();
        await TagMusic();

        await ClickWhenStable(Interests.Chip("Music"));

        await Expect(TagChange).ToHaveTextAsync("Music taken off the trip's tags.", new() { Timeout = 5000 });
        await Expect(Invitees).ToHaveTextAsync("Tag the trip to see whom to invite.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_trips_tags_name_the_residents_to_invite()
    {
        await NavigateAndBoot();
        await TagMusic();

        await ClickWhenStable(Interests.Chip("Art"));

        await Expect(Invitees).ToHaveTextAsync("Music and Art: invite Leah Kim, Mateo Reed and Edith Moreau.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_destination_tags_the_trip_with_what_it_suits_in_place_of_earlier_tags()
    {
        await NavigateAndBoot();
        await TagMusic();

        await Destination.Select("Garden Center");

        await Expect(Interests.SelectedChips).ToHaveTextAsync(new[] { "Gardening", "Walking" }, new() { Timeout = 5000 });
        await Expect(Invitees).ToHaveTextAsync("Gardening and Walking: invite Ava Stone, Nora Gray and Walter Chen.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_destination_repeats_the_trip_on_the_day_it_runs()
    {
        await NavigateAndBoot();

        await Destination.Select("Garden Center");

        await Expect(RepeatDays.SelectedChips).ToHaveTextAsync(new[] { "Thu" }, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_another_destination_moves_the_repeat_day_to_its_day()
    {
        await NavigateAndBoot();
        await ChooseGardenCenter();

        await Destination.Select("Symphony Matinee");

        await Expect(RepeatDays.SelectedChips).ToHaveTextAsync(new[] { "Sun" }, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_weekends_repeats_the_trip_on_sunday_and_saturday()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AddWeekends);

        await Expect(RepeatDays.SelectedChips).ToHaveTextAsync(new[] { "Sun", "Sat" }, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_resident_who_asked_to_go_puts_them_on_the_bus()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AskedToGo.Chip("Harold Price"));

        await Expect(Riders.Chips).ToHaveTextAsync(
            new[] { "Ava Stone", "Leah Kim", "Nora Gray", "Mateo Reed", "Walter Chen", "Harold Price" }, new() { Timeout = 5000 });
        await Expect(AskedToGo.Chips).ToHaveTextAsync(new[] { "Edith Moreau", "Ruth Alvarez" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_seated_rider_who_cancels_gives_the_seat_to_the_first_on_the_wait_list()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Riders.DeleteIcon("Leah Kim"));

        await Expect(SeatTaken).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SeatTaken).ToHaveTextAsync("Leah Kim is off the bus. Walter Chen comes off the wait list.");
        await Expect(Riders.Chips).ToHaveTextAsync(new[] { "Ava Stone", "Nora Gray", "Mateo Reed", "Walter Chen" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_who_leaves_the_wait_list_frees_no_seat()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Riders.DeleteIcon("Walter Chen"));

        await Expect(WaitListLeft).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(WaitListLeft).ToHaveTextAsync("Walter Chen leaves the wait list.");
        await Expect(SeatTaken).ToBeHiddenAsync();
        await Expect(SeatOpen).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_rider_who_cancels_with_nobody_waiting_leaves_a_seat_open()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Riders.DeleteIcon("Walter Chen"));
        await Expect(WaitListLeft).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Riders.DeleteIcon("Ava Stone"));

        await Expect(SeatOpen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SeatOpen).ToHaveTextAsync("Ava Stone is off the bus. A seat is open.");
        await Expect(SeatTaken).ToBeHiddenAsync();
        await Expect(WaitListLeft).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_lift_riders_highlights_the_residents_who_use_the_lift()
    {
        await NavigateAndBoot();

        await ClickWhenStable(MarkLiftRiders);

        await Expect(Riders.SelectedChips).ToHaveTextAsync(new[] { "Nora Gray", "Mateo Reed" }, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task with_the_lift_out_of_service_its_riders_come_off_the_bus()
    {
        await NavigateAndBoot();
        await ClickWhenStable(MarkLiftRiders);
        await Expect(Riders.SelectedChips).ToHaveTextAsync(new[] { "Nora Gray", "Mateo Reed" }, new() { Timeout = 5000 });

        await ClickWhenStable(LiftOutOfService);

        await Expect(Riders.Chips).ToHaveTextAsync(new[] { "Ava Stone", "Leah Kim", "Walter Chen" }, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task posting_the_trip_puts_its_destination_tags_and_repeat_days_on_the_calendar()
    {
        await NavigateAndBoot();
        await ChooseGardenCenter();
        await ClickWhenStable(AddWeekends);
        await Expect(RepeatDays.SelectedChips).ToHaveTextAsync(new[] { "Sun", "Thu", "Sat" }, new() { Timeout = 5000 });

        await ClickWhenStable(PostToCalendar);

        await Expect(PostStatus).ToHaveTextAsync(
            "Posted the Garden Center trip to the activities calendar, tagged gardening and walking, every Sunday, Thursday and Saturday.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task posting_without_a_destination_asks_where_the_trip_goes()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PostToCalendar);

        await Expect(PostStatus).ToHaveTextAsync("Choose where the trip goes before posting it.", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
