using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageNamesItsCulture(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAPageNamesItsCulture_the_page_renders_under_the_culture_its_address_names)]
    public async Task the_page_renders_under_the_culture_its_address_names()
    {
        var phonebook = await NewWorld();
        await phonebook.Page.GotoAsync(WorldPages.CultureSlipUnderTheGermanPhonebook);

        await Expect(phonebook.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("de-DE_phoneb 1.234,5");

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(WorldPages.CultureSlip);

        await Expect(unnamed.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("en-US 1,234.5");
    }

    [Test]
    [FailsFor(Promises.Harness.WhenAPageNamesItsCulture_an_address_that_names_a_culture_the_app_does_not_serve_is_answered_with_the_cultures_it_serves)]
    public async Task an_address_that_names_a_culture_the_app_does_not_serve_is_answered_with_the_cultures_it_serves()
    {
        var unserved = await NewWorld();
        await unserved.Page.GotoAsync(WorldPages.CultureSlipUnderAnUnservedCulture);

        await Expect(unserved.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The culture query names one of en-US, de-DE, tr-TR, haw-US, de-DE_phoneb and invariant, once.");

        var german = await NewWorld();
        await german.Page.GotoAsync(WorldPages.CultureSlipUnderGerman);

        await Expect(german.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("de-DE 1.234,5");
    }
}
