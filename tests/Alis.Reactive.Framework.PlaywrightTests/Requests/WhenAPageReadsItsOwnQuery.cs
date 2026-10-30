namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// The page's own query read through the class its action binds (<c>p.Url&lt;TQuery&gt;().Get(q =&gt; q.X)</c>), the
/// way MVC binds <c>[FromQuery]</c> (D3): a key matched ignoring case, a <c>+</c> read as a space and an encoded
/// plus as a plus, a scalar member reading the first value of a repeated key, and a list member every value.
/// </summary>
[TestFixture]
public sealed class WhenAPageReadsItsOwnQuery : AppPageTest
{
    private ILocator Wing => Page.Locator("#wing");

    private ILocator Wings => Page.Locator("#wings");

    [Test]
    public async Task a_key_matches_its_member_whatever_its_case()
    {
        await Open("/Requests/Directory?wING=Garden");

        await Expect(Wing).ToHaveTextAsync("Garden");
    }

    [Test]
    public async Task a_plus_reads_as_a_space()
    {
        await Open("/Requests/Directory?Wing=East+Garden");

        await Expect(Wing).ToHaveTextAsync("East Garden");
    }

    [Test]
    public async Task an_encoded_plus_reads_as_a_plus()
    {
        await Open("/Requests/Directory?Wing=East%2BGarden");

        await Expect(Wing).ToHaveTextAsync("East+Garden");
    }

    [Test]
    public async Task a_scalar_member_of_a_repeated_key_reads_its_first_value()
    {
        await Open("/Requests/Directory?Wing=East&wing=West");

        await Expect(Wing).ToHaveTextAsync("East");
    }

    [Test]
    public async Task a_list_member_reads_every_value_of_its_key_in_order()
    {
        await Open("/Requests/Directory?Wings=East&wings=West&WINGS=North");

        await Expect(Wings.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["East", "West", "North"]);
    }
}
