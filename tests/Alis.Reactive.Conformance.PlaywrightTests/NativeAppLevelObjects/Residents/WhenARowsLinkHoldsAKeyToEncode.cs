using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsLinkHoldsAKeyToEncode(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_35_0)]
    public async Task the_link_opens_the_address_its_click_requests()
    {
        var keys = await NewWorld();
        await keys.Page.GotoAsync(CarePages.ResidentKeys);
        var lovelace = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada Lovelace", Exact = true });
        var plus = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "a+b", Exact = true });
        var apostrophe = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "O'Neil", Exact = true });
        var parentheses = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Unit (2)", Exact = true });
        var tilde = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "~north", Exact = true });
        var accent = keys.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Zoé", Exact = true });

        var lovelaceSent = await keys.Page.RunAndWaitForRequestAsync(() => lovelace.ClickAsync(), "**/Residents/**");
        Assert.That(lovelaceSent.Url, Does.EndWith("/Residents/Ada%20Lovelace"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("Ada Lovelace");
        await Expect(lovelace).ToHaveAttributeAsync("href", "/Residents/Ada%20Lovelace");

        var plusSent = await keys.Page.RunAndWaitForRequestAsync(() => plus.ClickAsync(), "**/Residents/**");
        Assert.That(plusSent.Url, Does.EndWith("/Residents/a%2Bb"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("a+b");
        await Expect(plus).ToHaveAttributeAsync("href", "/Residents/a%2Bb");

        var apostropheSent = await keys.Page.RunAndWaitForRequestAsync(() => apostrophe.ClickAsync(), "**/Residents/**");
        Assert.That(apostropheSent.Url, Does.EndWith("/Residents/O%27Neil"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("O'Neil");
        await Expect(apostrophe).ToHaveAttributeAsync("href", "/Residents/O%27Neil");

        var parenthesesSent = await keys.Page.RunAndWaitForRequestAsync(() => parentheses.ClickAsync(), "**/Residents/**");
        Assert.That(parenthesesSent.Url, Does.EndWith("/Residents/Unit%20%282%29"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("Unit (2)");
        await Expect(parentheses).ToHaveAttributeAsync("href", "/Residents/Unit%20%282%29");

        var tildeSent = await keys.Page.RunAndWaitForRequestAsync(() => tilde.ClickAsync(), "**/Residents/**");
        Assert.That(tildeSent.Url, Does.EndWith("/Residents/~north"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("~north");
        await Expect(tilde).ToHaveAttributeAsync("href", "/Residents/~north");

        var accentSent = await keys.Page.RunAndWaitForRequestAsync(() => accent.ClickAsync(), "**/Residents/**");
        Assert.That(accentSent.Url, Does.EndWith("/Residents/Zo%C3%A9"));
        await Expect(keys.Page.Locator("#received")).ToHaveTextAsync("Zoé");
        await Expect(accent).ToHaveAttributeAsync("href", "/Residents/Zo%C3%A9");

        var letters = await NewWorld();
        await letters.Page.GotoAsync(CarePages.ResidentKeysOfLetters);
        var ada = letters.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true });
        var adaSent = await letters.Page.RunAndWaitForRequestAsync(() => ada.ClickAsync(), "**/Residents/**");
        Assert.That(adaSent.Url, Does.EndWith("/Residents/Ada"));
        await Expect(letters.Page.Locator("#received")).ToHaveTextAsync("Ada");
        await Expect(ada).ToHaveAttributeAsync("href", "/Residents/Ada");
    }
}
