namespace Alis.Reactive.Poc.PlaywrightTests.Presence;

[TestFixture]
public sealed class WhenAMealTrayIsServed : AppPageTest
{
    [Test]
    public async Task an_optional_value_the_tray_holds_reads_as_it_holds_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the portion", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#portion")).ToHaveTextAsync("Regular");
        await Expect(Page.Locator("#status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_value_the_older_tray_lacks_reads_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the portion on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-portion")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_value_written_to_the_tray_replaces_what_it_holds()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Write the portion", Exact = true }).ClickAsync();
        await Page.Locator("#tray").GetByRole(AriaRole.Button, new() { Name = "Show the portion", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#tray .tray-portion")).ToHaveTextAsync("Large");
    }

    [Test]
    public async Task an_optional_value_written_to_the_older_tray_is_set_though_it_lacks_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Write the portion on the older tray", Exact = true }).ClickAsync();
        await Page.Locator("#older-tray").GetByRole(AriaRole.Button, new() { Name = "Show the portion", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-tray .tray-portion")).ToHaveTextAsync("Large");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_write_only_value_written_to_the_tray_is_the_one_it_uses()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Write the slip", Exact = true }).ClickAsync();
        await Page.Locator("#tray").GetByRole(AriaRole.Button, new() { Name = "Print the slip", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#tray .tray-slip")).ToHaveTextAsync("Diabetic");
    }

    [Test]
    public async Task an_optional_write_only_value_written_to_the_older_tray_is_set_for_it_to_use()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Write the slip on the older tray", Exact = true }).ClickAsync();
        await Page.Locator("#older-tray").GetByRole(AriaRole.Button, new() { Name = "Print the slip", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-tray .tray-slip")).ToHaveTextAsync("Diabetic");
    }

    [Test]
    public async Task an_optional_live_object_the_tray_holds_reads_its_members()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the cover", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#cover")).ToHaveTextAsync("Cloche");
    }

    [Test]
    public async Task an_optional_live_object_the_older_tray_lacks_reads_nothing_through_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the cover on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-cover")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_live_list_the_tray_holds_reads_its_items()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first side", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#side")).ToHaveTextAsync("Rice");
    }

    [Test]
    public async Task an_optional_live_list_the_older_tray_lacks_reads_nothing_at_an_index()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first side on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-side")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_method_the_tray_has_runs()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Warm the tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#tray .tray-state")).ToHaveTextAsync("warmed");
        await Expect(Page.Locator("#status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_method_the_older_tray_lacks_runs_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Warm the tray on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
        await Expect(Page.Locator("#older-tray .tray-state")).ToHaveTextAsync("cold");
    }

    [Test]
    public async Task an_optional_method_the_unset_tray_holds_as_undefined_runs_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Warm the tray on the unset tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#unset-status")).ToHaveTextAsync("Done");
        await Expect(Page.Locator("#unset-tray .tray-state")).ToHaveTextAsync("cold");
    }

    [Test]
    public async Task an_optional_method_the_tray_has_returns_its_result()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read when it was served", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#served")).ToHaveTextAsync("12:30");
    }

    [Test]
    public async Task an_optional_method_the_older_tray_lacks_returns_nothing()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read when it was served on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-served")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_method_the_unset_tray_holds_as_undefined_returns_nothing()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read when it was served on the unset tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#unset-served")).ToHaveTextAsync("");
        await Expect(Page.Locator("#unset-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_awaited_method_the_tray_has_runs_before_the_next_step()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Reheat the tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#status")).ToHaveTextAsync("Done");
        await Expect(Page.Locator("#tray .tray-state")).ToHaveTextAsync("reheated");
    }

    [Test]
    public async Task an_optional_awaited_method_the_older_tray_lacks_runs_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Reheat the tray on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
        await Expect(Page.Locator("#older-tray .tray-state")).ToHaveTextAsync("cold");
    }

    [Test]
    public async Task an_optional_awaited_method_the_unset_tray_holds_as_undefined_runs_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Reheat the tray on the unset tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#unset-status")).ToHaveTextAsync("Done");
        await Expect(Page.Locator("#unset-tray .tray-state")).ToHaveTextAsync("cold");
    }

    [Test]
    public async Task an_optional_awaited_result_the_tray_has_reads_its_result()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the temperature", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#temperature")).ToHaveTextAsync("68°F");
    }

    [Test]
    public async Task an_optional_awaited_result_the_older_tray_lacks_reads_nothing()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the temperature on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-temperature")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_awaited_result_the_unset_tray_holds_as_undefined_reads_nothing()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the temperature on the unset tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#unset-temperature")).ToHaveTextAsync("");
        await Expect(Page.Locator("#unset-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_returned_object_the_tray_has_reads_its_members()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the lid", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#lid")).ToHaveTextAsync("Steel lid");
    }

    [Test]
    public async Task an_optional_returned_object_the_older_tray_lacks_reads_nothing_through_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the lid on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-lid")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_returned_object_the_unset_tray_holds_as_undefined_reads_nothing_through_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the lid on the unset tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#unset-lid")).ToHaveTextAsync("");
        await Expect(Page.Locator("#unset-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_returned_list_the_tray_has_reads_its_items()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first extra", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#extra")).ToHaveTextAsync("Bread");
    }

    [Test]
    public async Task an_optional_returned_list_the_older_tray_lacks_reads_nothing_at_an_index()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first extra on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-extra")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_callback_answered_on_the_tray_replaces_its_own()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the garnish", Exact = true }).ClickAsync();
        await Page.Locator("#tray").GetByRole(AriaRole.Button, new() { Name = "Plate the dish", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#tray .tray-plated")).ToHaveTextAsync("Soup");
    }

    [Test]
    public async Task an_optional_callback_answered_on_the_older_tray_is_set_though_it_lacks_one()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Answer the garnish on the older tray", Exact = true }).ClickAsync();
        await Page.Locator("#older-tray").GetByRole(AriaRole.Button, new() { Name = "Plate the dish", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-tray .tray-plated")).ToHaveTextAsync("Soup");
    }

    [Test]
    public async Task an_optional_member_of_a_required_object_the_tray_holds_reads_its_value()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the server's badge", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#badge")).ToHaveTextAsync("Ines");
    }

    [Test]
    public async Task an_optional_member_of_a_required_object_the_older_tray_holds_as_null_stops_the_pipeline()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the server's badge on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Reading");
        await Expect(Page.Locator("#older-badge")).ToHaveTextAsync("unread");
    }

    [Test]
    public async Task a_nullable_live_object_the_tray_holds_reads_its_members()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the relief server's badge", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#relief")).ToHaveTextAsync("Omar");
    }

    [Test]
    public async Task a_nullable_live_object_the_older_tray_holds_as_null_reads_nothing_through_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the relief server's badge on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-relief")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task a_nullable_returned_object_the_tray_returns_reads_its_members()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the spare lid", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#spare-lid")).ToHaveTextAsync("Glass lid");
    }

    [Test]
    public async Task a_nullable_returned_object_the_older_tray_returns_as_null_reads_nothing_through_it()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the spare lid on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-spare-lid")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task a_nullable_live_list_the_tray_holds_reads_its_items()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first side order", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#side-order")).ToHaveTextAsync("Soup");
    }

    [Test]
    public async Task a_nullable_live_list_the_older_tray_holds_as_null_reads_nothing_at_an_index()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first side order on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-side-order")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task a_nullable_returned_list_the_tray_returns_reads_its_items()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first spare side", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#spare-side")).ToHaveTextAsync("Fruit");
    }

    [Test]
    public async Task a_nullable_returned_list_the_older_tray_returns_as_null_reads_nothing_at_an_index()
    {
        await Open("/Trays/Presence");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the first spare side on the older tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-spare-side")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_payload_member_the_tray_hands_reads_its_value()
    {
        await Open("/Trays/Presence");

        await Page.Locator("#tray").GetByRole(AriaRole.Button, new() { Name = "Deliver the tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#note")).ToHaveTextAsync("No salt");
        await Expect(Page.Locator("#status")).ToHaveTextAsync("Done");
    }

    [Test]
    public async Task an_optional_payload_member_the_older_tray_omits_reads_nothing()
    {
        await Open("/Trays/Presence");

        await Page.Locator("#older-tray").GetByRole(AriaRole.Button, new() { Name = "Deliver the tray", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#older-note")).ToHaveTextAsync("");
        await Expect(Page.Locator("#older-status")).ToHaveTextAsync("Done");
    }
}
