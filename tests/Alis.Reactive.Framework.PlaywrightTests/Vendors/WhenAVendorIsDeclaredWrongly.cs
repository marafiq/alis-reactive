namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// A vendor a page declares wrongly, refused when the page renders with the message the developer reads: a
/// name that does not start with a letter, an object path with an empty step, two different vendors given one
/// name on one plan, and one element named as the components of two vendors.
/// </summary>
[TestFixture]
public sealed class WhenAVendorIsDeclaredWrongly : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Vendors/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_vendor_named_with_a_leading_digit_is_refused()
    {
        await Render("NameAVendorStartingWithADigit");

        await Expect(Message(
            "Vendor name '1slate' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_object_path_with_an_empty_step_is_refused()
    {
        await Render("PathAnObjectWithAStrayDot");

        await Expect(Message(
            "Path 'slate.' contains an empty segment at index 1. Use dot-separated property names without consecutive, leading, or trailing dots.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task two_different_vendors_given_one_name_are_refused()
    {
        await Render("NameTwoVendorsAlike");

        await Expect(Message("Two different vendors are named 'slate'. Give each vendor its own name.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_element_named_as_the_components_of_two_vendors_is_refused()
    {
        await Render("ReadTheSlateAsAMeter");

        await Expect(Message(
            "Element 'slate' is named as a component of vendor 'slate' and as one of vendor 'meter', but an element holds one component: name it by the declaration of the component its view renders.")).ToBeVisibleAsync();
    }
}
