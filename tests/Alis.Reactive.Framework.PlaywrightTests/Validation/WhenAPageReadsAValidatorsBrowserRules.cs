namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// The browser rules a validator declares, read back by a page as a component whose own JavaScript validates
/// reads them (<c>plan.BrowserRules&lt;TValidator&gt;()</c>): each member with its rules by their classes, and
/// whether each rule always applies or only while its condition holds.
/// </summary>
[TestFixture]
public sealed class WhenAPageReadsAValidatorsBrowserRules : AppPageTest
{
    private ILocator Rules => Page.Locator("#rules");

    [Test]
    public async Task each_member_is_read_back_with_its_rules_by_their_kinds()
    {
        await Open("/Validation/RulesReadBack");

        await Expect(Rules.GetByText("Visitor: not empty, always")).ToBeVisibleAsync();
        await Expect(Rules.GetByText("Notes: at most 20 characters, always")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_range_read_back_gives_its_bounds_as_numbers()
    {
        await Open("/Validation/RulesReadBack");

        await Expect(Rules.GetByText("Guests: from 1 to 4, always")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_comparison_read_back_gives_its_fixed_value_as_a_number_only_when_it_is_one()
    {
        await Open("/Validation/RulesReadBack");

        await Expect(Page.Locator("#sameness-rules").GetByText("Visitors: compared with 5")).ToBeVisibleAsync();
        await Expect(Page.Locator("#sameness-rules").GetByText("Arrives: compared with a value that is no number")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_rule_read_back_says_whether_it_always_applies_or_only_while_its_condition_holds()
    {
        await Open("/Validation/RulesReadBack");

        await Expect(Rules.GetByText("Company: not empty, only while its condition holds")).ToBeVisibleAsync();
        await Expect(Rules.GetByText("Visitor: not empty, always")).ToBeVisibleAsync();
    }
}
