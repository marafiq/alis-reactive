using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARoomNumberIsCheckedByItsPattern(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_10_0)]
    public async Task arabic_indic_digits_fail_the_pattern_of_digits_zero_to_nine_and_nothing_is_sent()
    {
        var arabicIndic = await NewWorld();
        await arabicIndic.Page.GotoAsync(CarePages.RoomNumbering);
        await arabicIndic.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room number", Exact = true }).FillAsync("١٢");
        await arabicIndic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(arabicIndic.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room number", Exact = true })
            .GetByText("Write the room number in digits 0 to 9.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(arabicIndic.Page.Locator("#saved")).ToBeEmptyAsync();

        var ascii = await NewWorld();
        await ascii.Page.GotoAsync(CarePages.RoomNumbering);
        await ascii.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room number", Exact = true }).FillAsync("12");
        await ascii.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ascii.Page.Locator("#saved")).ToHaveTextAsync("12");
        await Expect(ascii.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
