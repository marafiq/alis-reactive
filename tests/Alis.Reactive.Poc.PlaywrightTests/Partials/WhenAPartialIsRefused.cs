namespace Alis.Reactive.Poc.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenAPartialIsRefused : AppPageTest
{
    [Test]
    public async Task a_partial_for_a_foreach_item_instead_of_the_model_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Families/ContactsByForeach");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A partial's lambda reads its member from a value the view holds, not from the view's model, as m => contact over a foreach's item does, so MVC gives the partial no prefix and its inputs post under no member: render each item by its position, as Html.PartialFor(m => m.Contacts[i], view) in a for loop.");
    }

    [Test]
    public async Task a_partial_for_a_member_reached_through_a_call_that_is_no_indexer_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Families/MobileContact");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.Contacts.Find(contact => (contact.Kind == \"Mobile\"))' renders no member of the view's model. A partial is rendered for a member the model reaches through members, list positions and indexers, as m => m.Contacts[0], or for the model itself, m => m.");
    }
}
