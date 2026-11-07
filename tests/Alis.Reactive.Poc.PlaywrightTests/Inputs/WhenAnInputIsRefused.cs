namespace Alis.Reactive.Poc.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAnInputIsRefused : AppPageTest
{
    [Test]
    public async Task an_input_bound_to_the_whole_model_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/WholeModel");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m' binds no member. An input is bound to a member of the view's model, and its id is scoped by the class that declares it: m => m.Name.");
    }

    [Test]
    public async Task an_input_under_a_prefix_its_view_took_after_its_plan_started_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/LatePrefix");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: The view's field prefix changed from none to 'Resident' after its plan started: MVC names input 'm => m.ResidentName' under the new prefix and the plan under the one it started with. Give the view its prefix before its plan starts.");
    }

    [Test]
    public async Task an_input_bound_to_a_member_a_generic_class_declares_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Admissions/TaggedBadge");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'm => m.Badge.Value' binds a member that a generic class declares, whose full name carries its type arguments' assemblies and versions, so the id it scopes would change with them: bind a member a non-generic class declares.");
    }
}
