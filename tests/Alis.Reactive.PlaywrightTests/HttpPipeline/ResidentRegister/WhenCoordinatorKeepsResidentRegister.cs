using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentRegister;

/// <summary>
/// As the Maple wing front-desk coordinator, I want to find residents, move a resident to another room,
/// remove duplicate records and prepare the fire-drill roster from one register, so that the drill counts
/// every resident once, in the room they live in.
/// </summary>
/// <remarks>
/// The register holds five records for three residents: Helen Park (1042), Arthur Bell (1043) and Rosa
/// Diaz (1044, the newest resident) each hold a care plan; 1047 (Helen Park) and 1048 (Arthur Bell) are
/// duplicates entered by mistake. The server never removes a record that holds a care plan, and moves a
/// resident only into a room no other resident lives in. A removal or a move reloads the list of records
/// from the server. The roster link always names the Maple wing, which the server always accepts, so it
/// has no error route. A modified click on a link (a new tab) stays the browser's and is not exercised
/// here: a record's address answers only the removal. The page's inputs are found by the framework's id
/// rule rather than PagePlan: each record's link carries a plan of its own, rendered before the page's,
/// and PagePlan reads the first plan on the page.
/// </remarks>
[TestFixture]
public class WhenCoordinatorKeepsResidentRegister : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ResidentRegister";
    private const string RegisterUrl = "**/Sandbox/HttpPipeline/ResidentRegister";
    private const string Refused = "the server responded with a status of 409";
    private const string Incomplete = "the server responded with a status of 422";

    private static readonly Regex Warning = new(@"\btext-danger\b");

    private SemaphoreSlim _released = null!;

    private ILocator Field(Expression<Func<ResidentRegisterModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator Loading => Page.Locator("#register-loading");
    private ILocator RecordCount => Page.Locator("#register-record-count");
    private ILocator ResidentCount => Page.Locator("#register-resident-count");
    private ILocator NewestResident => Page.Locator("#register-newest");
    private ILocator Find => Page.Locator("#find-resident-btn");
    private ILocator SearchCount => Page.Locator("#search-count");
    private ILocator SearchMatches => Page.Locator("#search-matches");
    private ILocator MoveResident => Page.Locator("#move-resident-btn");
    private ILocator MoveStatus => Page.Locator("#move-status");
    private ILocator Record(int recordId) => Page.Locator($"#record-{recordId}");
    private ILocator RecordRoom(int recordId) => Page.Locator($"#record-{recordId}-room");
    private ILocator RemoveDuplicate(int recordId) => Page.Locator($"#remove-record-{recordId}");
    private ILocator RegisterStatus => Page.Locator("#register-status");
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");
    private ILocator ConfirmQuestion => ConfirmDialog.Locator(".e-dlg-content");
    private ILocator PrepareRoster => Page.Locator("#prepare-roster-link");
    private ILocator RosterStatus => Page.Locator("#roster-status");
    private ILocator RosterSheet => Page.Locator("#roster-sheet");

    private async Task OpenRegister() => await NavigateToAndWaitForBoot(Path);

    // The request is held at the browser until the test releases it, then sent on unchanged, so what the
    // page shows before the answer arrives can be seen.
    private async Task HoldUntilReleased(string url)
    {
        _released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            await _released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
    }

    private async Task Answer(string button) =>
        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = button }));

    private async Task MoveTo(string resident, string room)
    {
        await Field(m => m.MoveResidentId).SelectOptionAsync(new SelectOptionValue { Label = resident });
        await Field(m => m.MoveToRoom).FillAsync(room);
        await ClickWhenStable(MoveResident);
    }

    [Test]
    public async Task the_register_says_it_is_loading_until_its_counts_arrive()
    {
        await HoldUntilReleased(RegisterUrl + "/Summary");
        await OpenRegister();

        await Expect(Loading).ToBeVisibleAsync();
        await Expect(RecordCount).ToHaveTextAsync("—");
        _released.Release();

        await Expect(RecordCount).ToHaveTextAsync("5");
        await Expect(ResidentCount).ToHaveTextAsync("3");
        await Expect(NewestResident).ToHaveTextAsync("Rosa Diaz");
        await Expect(Loading).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_part_of_a_name_lists_every_record_that_holds_it()
    {
        await OpenRegister();
        await Field(m => m.SearchName).FillAsync("park");

        await ClickWhenStable(Find);

        await Expect(SearchMatches).ToHaveTextAsync("Helen Park, Maple 214 (record 1042); Helen Park, Maple 214 (record 1047)");
        await Expect(SearchCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_a_name_nobody_on_the_register_has_says_no_resident_matches()
    {
        await OpenRegister();
        await Field(m => m.SearchName).FillAsync("Smith");

        await ClickWhenStable(Find);

        await Expect(SearchMatches).ToHaveTextAsync("No resident on the register matches Smith.");
        await Expect(SearchCount).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_a_resident_to_an_open_room_says_where_they_now_live_and_updates_their_record()
    {
        await OpenRegister();

        await MoveTo("Helen Park", "Maple 220");

        await Expect(MoveStatus).ToHaveTextAsync("Helen Park now lives in Maple 220.");
        await Expect(RecordRoom(1042)).ToHaveTextAsync("Maple 220");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_a_resident_into_another_residents_room_names_who_lives_there()
    {
        await OpenRegister();

        await MoveTo("Helen Park", "Maple 216");

        await Expect(MoveStatus).ToHaveTextAsync("Maple 216 is Arthur Bell's room.");
        await Expect(MoveStatus).ToHaveClassAsync(Warning);
        await Expect(RecordRoom(1042)).ToHaveTextAsync("Maple 214");
        AssertNoConsoleErrorsExcept(Refused);
    }

    [Test]
    public async Task moving_a_resident_without_naming_the_room_asks_for_it()
    {
        await OpenRegister();

        await MoveTo("Helen Park", "");

        await Expect(MoveStatus).ToHaveTextAsync("Choose a resident and the room they move to.");
        await Expect(MoveStatus).ToHaveClassAsync(Warning);
        AssertNoConsoleErrorsExcept(Incomplete);
    }

    [Test]
    public async Task a_move_to_an_open_room_after_a_refused_one_clears_the_warning()
    {
        await OpenRegister();
        await MoveTo("Helen Park", "Maple 216");
        await Expect(MoveStatus).ToHaveTextAsync("Maple 216 is Arthur Bell's room.");

        await Field(m => m.MoveToRoom).FillAsync("Maple 220");
        await ClickWhenStable(MoveResident);

        await Expect(MoveStatus).ToHaveTextAsync("Helen Park now lives in Maple 220.");
        await Expect(MoveStatus).Not.ToHaveClassAsync(Warning);
        AssertNoConsoleErrorsExcept(Refused);
    }

    [Test]
    public async Task removing_a_duplicate_record_takes_it_off_the_register()
    {
        await OpenRegister();
        await Expect(Record(1047)).ToBeVisibleAsync();

        await ClickWhenStable(RemoveDuplicate(1047));
        await Expect(ConfirmQuestion).ToHaveTextAsync("Remove record 1047 for Helen Park as a duplicate entry?");
        await Answer("OK");

        await Expect(RegisterStatus).ToHaveTextAsync("Removed record 1047 for Helen Park: duplicate entry.");
        await Expect(Record(1047)).ToHaveCountAsync(0);
        await Expect(Record(1042)).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_register_says_it_is_removing_a_record_until_the_server_answers()
    {
        await HoldUntilReleased(RegisterUrl + "/Records/1047");
        await OpenRegister();
        await ClickWhenStable(RemoveDuplicate(1047));

        await Answer("OK");

        await Expect(RegisterStatus).ToHaveTextAsync("Removing record 1047...");
        await Expect(Record(1047)).ToBeVisibleAsync();
        _released.Release();
        await Expect(RegisterStatus).ToHaveTextAsync("Removed record 1047 for Helen Park: duplicate entry.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task declining_to_remove_a_record_keeps_it_and_sends_nothing()
    {
        await OpenRegister();
        var removals = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "DELETE") removals.Enqueue(request.Url);
        };
        await ClickWhenStable(RemoveDuplicate(1047));

        await Answer("Cancel");

        await Expect(RegisterStatus).ToHaveTextAsync("Kept record 1047 for Helen Park.");
        await Expect(Record(1047)).ToBeVisibleAsync();
        Assert.That(removals, Is.Empty, "A declined removal sends nothing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_record_that_holds_a_care_plan_is_kept_when_its_removal_is_confirmed()
    {
        await OpenRegister();
        await ClickWhenStable(RemoveDuplicate(1043));

        await Answer("OK");

        await Expect(RegisterStatus).ToHaveTextAsync("Arthur Bell's record 1043 holds a care plan and was kept.");
        await Expect(Record(1043)).ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept(Refused);
    }

    // The list is reloaded from the server after a removal, with each record's link in it again.
    [Test]
    public async Task after_the_list_reloads_the_next_duplicate_can_be_removed()
    {
        await OpenRegister();
        await ClickWhenStable(RemoveDuplicate(1047));
        await Answer("OK");
        await Expect(Record(1047)).ToHaveCountAsync(0);

        await ClickWhenStable(RemoveDuplicate(1048));
        await Expect(ConfirmQuestion).ToHaveTextAsync("Remove record 1048 for Arthur Bell as a duplicate entry?");
        await Answer("OK");

        await Expect(RegisterStatus).ToHaveTextAsync("Removed record 1048 for Arthur Bell: duplicate entry.");
        await Expect(Record(1048)).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task preparing_the_fire_drill_roster_lists_every_record_on_the_wing_by_room()
    {
        await OpenRegister();

        await ClickWhenStable(PrepareRoster);

        await Expect(RosterStatus).ToHaveTextAsync("The Maple wing roster is ready to print.");
        await Expect(RosterSheet.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Maple wing fire-drill roster");
        await Expect(RosterSheet.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Maple 214: Helen Park",
            "Maple 214: Helen Park",
            "Maple 216: Arthur Bell",
            "Maple 216: Arthur Bell",
            "Maple 218: Rosa Diaz",
        });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_roster_says_it_is_being_prepared_until_it_arrives()
    {
        await HoldUntilReleased(RegisterUrl + "/Roster");
        await OpenRegister();

        await ClickWhenStable(PrepareRoster);

        await Expect(RosterStatus).ToHaveTextAsync("Preparing the Maple wing roster...");
        await Expect(RosterSheet).ToHaveTextAsync("");
        _released.Release();
        await Expect(RosterStatus).ToHaveTextAsync("The Maple wing roster is ready to print.");
        await Expect(RosterSheet.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Maple wing fire-drill roster");
        AssertNoConsoleErrors();
    }
}
