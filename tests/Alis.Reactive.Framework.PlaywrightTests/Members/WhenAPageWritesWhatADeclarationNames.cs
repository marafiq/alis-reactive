namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// What a declaration names, written into a page as text: a field reads as the name the component's
/// JavaScript gives it, and a vendor as its name.
/// </summary>
[TestFixture]
public sealed class WhenAPageWritesWhatADeclarationNames : AppPageTest
{
    [Test]
    public async Task a_field_written_into_the_page_reads_as_its_javascript_name()
    {
        await Open("/Members/Names");

        await Expect(Page.Locator("#sort-field")).ToHaveTextAsync("due");
    }

    [Test]
    public async Task a_vendor_written_into_the_page_reads_as_its_name()
    {
        await Open("/Members/Names");

        await Expect(Page.Locator("#slate-vendor")).ToHaveTextAsync("slate");
    }
}
