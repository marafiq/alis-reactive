using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormSendsTwoDocumentsBesideTheirCaptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_27_0)]
    public async Task the_endpoint_binds_each_file_with_its_name_its_size_and_the_caption_of_its_row()
    {
        var inTheirRows = await NewWorld();
        await inTheirRows.Page.GotoAsync(CarePages.IntakeDocuments);
        await inTheirRows.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Byron");
        await inTheirRows.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First document", Exact = true })
            .GetByLabel("File", new LocatorGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "consent.pdf", MimeType = "application/pdf", Buffer = new byte[1024] });
        await inTheirRows.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First document", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Caption", Exact = true }).FillAsync("Consent");
        await inTheirRows.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second document", Exact = true })
            .GetByLabel("File", new LocatorGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "Überweisung.pdf", MimeType = "application/pdf", Buffer = new byte[2048] });
        await inTheirRows.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second document", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Caption", Exact = true }).FillAsync("Referral");
        await inTheirRows.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(inTheirRows.Page.Locator("#saved")).ToHaveTextAsync("Ada Byron; consent.pdf 1024 Consent; Überweisung.pdf 2048 Referral");

        var exchanged = await NewWorld();
        await exchanged.Page.GotoAsync(CarePages.IntakeDocuments);
        await exchanged.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Byron");
        await exchanged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First document", Exact = true })
            .GetByLabel("File", new LocatorGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "Überweisung.pdf", MimeType = "application/pdf", Buffer = new byte[2048] });
        await exchanged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First document", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Caption", Exact = true }).FillAsync("Consent");
        await exchanged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second document", Exact = true })
            .GetByLabel("File", new LocatorGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "consent.pdf", MimeType = "application/pdf", Buffer = new byte[1024] });
        await exchanged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second document", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Caption", Exact = true }).FillAsync("Referral");
        await exchanged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(exchanged.Page.Locator("#saved")).ToHaveTextAsync("Ada Byron; Überweisung.pdf 2048 Consent; consent.pdf 1024 Referral");
    }
}
