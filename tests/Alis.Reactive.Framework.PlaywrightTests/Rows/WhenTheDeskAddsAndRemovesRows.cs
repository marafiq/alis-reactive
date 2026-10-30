namespace Alis.Reactive.Framework.PlaywrightTests.Rows;

/// <summary>
/// Dynamic rows as ADR-0003 writes them: the list is one partial in a slot, and adding or removing a contact posts
/// the list and loads the server's rendering of it again, its rows numbered from 0. What the rows hold survives the
/// round trip, and a removed row leaves no gap in what the sheet sends, as JSON or as a form post.
/// </summary>
[TestFixture]
public sealed class WhenTheDeskAddsAndRemovesRows : AppPageTest
{
    private ILocator Saved => Page.Locator("#saved");

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private Task Click(string button) => Page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();

    [Test]
    public async Task adding_a_row_keeps_what_the_rows_hold()
    {
        await Open("/Rows/Contacts");
        await Input("Contact 2").FillAsync("Di");

        await Click("Add a contact");
        await Input("Contact 3").FillAsync("Eve");
        await Click("Save the contacts");

        await Expect(Saved).ToHaveTextAsync("Ada's contacts: Bo, Di, Eve");
    }

    [TestCase("Save the contacts", TestName = "removing_a_middle_row_sends_the_rest_with_no_gap_as_json")]
    [TestCase("Post the contacts as a form", TestName = "removing_a_middle_row_posts_the_rest_with_no_gap_as_a_form")]
    public async Task removing_a_middle_row_sends_the_rest_with_no_gap(string save)
    {
        await Open("/Rows/Contacts");
        await Click("Add a contact");
        await Input("Contact 3").FillAsync("Di");

        await Click("Remove contact 2");
        await Expect(Input("Contact 3")).ToHaveCountAsync(0);
        await Click(save);

        await Expect(Saved).ToHaveTextAsync("Ada's contacts: Bo, Di");
    }
}
