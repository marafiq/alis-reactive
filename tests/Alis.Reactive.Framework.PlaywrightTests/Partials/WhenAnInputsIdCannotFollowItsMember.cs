namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenAnInputsIdCannotFollowItsMember : AppPageTest
{
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Partials/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_partial_typed_to_an_interface_is_refused_naming_the_fix()
    {
        await Render("TypeAPartialToAnInterface");

        await Expect(Message(
            "Input 'Visitor' binds a member that INamedVisitor declares. An input's id is scoped by the class that declares " +
            "its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a " +
            "base class, that declares it.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_input_bound_to_the_views_model_itself_is_refused_naming_the_fix()
    {
        await Render("BindAnInputToTheModelItself");

        await Expect(Message(
            "m => m binds no member. An input's id is scoped by the class that declares the member it binds: render " +
            "the input in a view of that class, like m => m.Name.")).ToBeVisibleAsync();
    }
}
