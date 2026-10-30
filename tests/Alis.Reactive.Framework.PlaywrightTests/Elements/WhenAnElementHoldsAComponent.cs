namespace Alis.Reactive.Framework.PlaywrightTests.Elements;

/// <summary>
/// An element and the component on it are two objects of the plan, whichever the view names first: a step on
/// <c>p.Element(id)</c> reaches the element the view rendered, and a step on <c>p.Component&lt;T&gt;(id)</c> reaches
/// the component, the slate on the round board.
/// </summary>
[TestFixture]
public sealed class WhenAnElementHoldsAComponent : AppPageTest
{
    private ILocator Board => Page.Locator("#round-board");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task an_element_named_before_the_component_on_it_is_hidden_by_its_own_step()
    {
        await Open("/Elements/RoundBoard?first=Element");
        await Expect(Board).ToBeVisibleAsync();

        await Click("Hide the board");

        await Expect(Board).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_component_named_after_the_element_holding_it_is_retitled_by_its_own_step()
    {
        await Open("/Elements/RoundBoard?first=Element");

        await Click("Retitle the board");

        await Expect(Board.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Evening round");
    }

    [Test]
    public async Task an_element_named_after_the_component_on_it_is_hidden_by_its_own_step()
    {
        await Open("/Elements/RoundBoard?first=Component");
        await Expect(Board).ToBeVisibleAsync();

        await Click("Hide the board");

        await Expect(Board).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_component_named_before_the_element_holding_it_is_retitled_by_its_own_step()
    {
        await Open("/Elements/RoundBoard?first=Component");

        await Click("Retitle the board");

        await Expect(Board.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Evening round");
    }
}
