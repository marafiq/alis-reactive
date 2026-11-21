using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormSendsABoundListOfFiles(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_27_1)]
    public async Task the_endpoint_binds_the_selected_files_in_order_and_no_files_from_an_empty_selection()
    {
        var selected = await NewWorld();
        await selected.Page.GotoAsync(CarePages.IntakeAttachments);
        await selected.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Byron");
        await selected.Page.GetByLabel("Attachments", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync([
            new FilePayload { Name = "consent.pdf", MimeType = "application/pdf", Buffer = new byte[1024] },
            new FilePayload { Name = "Überweisung.pdf", MimeType = "application/pdf", Buffer = new byte[2048] },
        ]);
        await selected.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(selected.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved", Exact = true }))
            .ToHaveTextAsync("Ada Byron; 2 attachments; consent.pdf 1024; Überweisung.pdf 2048");

        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.IntakeAttachments);
        await empty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Byron");
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved", Exact = true }))
            .ToHaveTextAsync("Ada Byron; 0 attachments");
    }
}
