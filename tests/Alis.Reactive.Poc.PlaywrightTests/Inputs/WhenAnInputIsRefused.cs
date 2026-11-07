namespace Alis.Reactive.Poc.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAnInputIsRefused : AppPageTest
{
    [Test]
    public async Task an_input_bound_to_the_whole_model_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/WholeModel");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    [Test]
    public async Task an_input_bound_through_a_method_of_a_list_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/FirstVisitor");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.Visitors.First().Name' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    [Test]
    public async Task an_input_bound_through_a_call_that_is_no_indexer_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/PrimaryVisitor");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.Visitors.Find(visitor => (visitor.Name == \"Ada Byron\")).Name' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    [Test]
    public async Task an_input_bound_to_a_call_mvc_cannot_name_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/ShortenedResident");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.ResidentName.Substring(1)' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    [Test]
    public async Task an_input_bound_to_a_foreach_item_instead_of_the_model_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/VisitorsByForeach");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop.");
    }

    [Test]
    public async Task an_input_under_a_prefix_its_view_took_after_its_plan_started_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/LatePrefix");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: The view's field prefix changed from none to 'Resident' after its plan started: MVC names input 'm => m.ResidentName' under the new prefix and the plan under the one it started with. Render the view for its member with Html.PartialFor, which gives it its prefix before its plan starts.");
    }

    [Test]
    public async Task an_input_bound_to_a_member_a_generic_class_declares_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/TaggedBadge");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.Badge.Value' binds a member that a generic class declares, whose full name carries its type arguments' assemblies and versions, so the id it scopes would change with them: bind a member a non-generic class declares.");
    }
}
