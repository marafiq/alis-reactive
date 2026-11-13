using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHiddenFieldsValueIsWritten(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F6.M24_X2_0)]
    public async Task the_write_raises_the_fields_change()
    {
        var assigned = await NewWorld();
        await assigned.Page.GotoAsync(CarePages.IntakeBed);
        await assigned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assign B-12", Exact = true }).ClickAsync();
        await Expect(assigned.Page.Locator("#bed-echo")).ToHaveTextAsync("B-12");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(CarePages.IntakeBed);
        await Expect(untouched.Page.Locator("#bed-echo")).ToHaveTextAsync("");
    }
}
