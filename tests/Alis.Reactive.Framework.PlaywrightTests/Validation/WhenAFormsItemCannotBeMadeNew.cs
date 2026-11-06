namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAFormsItemCannotBeMadeNew : AppPageTest
{
    [Test]
    public async Task a_form_whose_items_have_no_constructor_to_bind_through_is_refused_naming_the_type_and_the_way_out()
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/LedgerRefused");

        await Expect(Page.GetByText(
            "A form's check reads a member its request does not send as a new LedgerLine leaves it, but LedgerLine has " +
            "neither a public parameterless constructor nor a single public constructor, so the endpoint cannot bind one " +
            "into Ledger either. Give LedgerLine a public parameterless constructor.").First).ToBeVisibleAsync();
    }
}
