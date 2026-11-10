namespace Alis.Reactive.Poc.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenAViewsPrefixMovesInsideItsPlan : AppPageTest
{
    [Test]
    public async Task a_plan_whose_view_took_another_prefix_before_its_end_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Families/FamilyPrefixMovedInsidePlan");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: The view's field prefix changed from none to 'NextOfKin' inside its plan, so what the view renders after the change posts under names its plan does not send or check: render a member's view with Html.PartialFor, which leaves the view's prefix as it is.");
    }
}
