using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorDesignsAnAdmissionForm(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F54.M21_X1401_0)]
    public async Task the_saved_hospital_admission_requires_the_receiving_nurse()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.FormDesign);
        await desk.Page.GetByRole(AriaRole.Group, new() { Name = "Receiving nurse textbox", Exact = true }).PressAsync("Enter");
        await desk.Page.GetByRole(AriaRole.Tab, new() { Name = "Validation", Exact = true }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Checkbox, new() { Name = "Required checkbox", Exact = true }).PressAsync("Space");
        await Expect(desk.Page.GetByRole(AriaRole.Status, new() { Name = "Template changes" })).ToHaveTextAsync("Receiving nurse is required.");

        await desk.Page.GetByRole(AriaRole.Link, new() { Name = "Preview saved form" }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Button, new() { Name = "Submit admission" }).ClickAsync();
        await Expect(desk.Page.GetByText("Enter the receiving nurse.", new() { Exact = true })).ToBeVisibleAsync();

        await desk.Page.GetByRole(AriaRole.Textbox, new() { Name = "Receiving nurse textbox", Exact = true }).FillAsync("Elena Ruiz");
        await desk.Page.GetByRole(AriaRole.Button, new() { Name = "Submit admission" }).ClickAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Status, new() { Name = "Admission preview result" })).ToHaveTextAsync("Hospital admission passed.");
    }

    [Test]
    [FailsFor(Promises.M21_F54.M21_X1402_0)]
    public async Task the_reopened_draft_shows_transfer_notes_only_for_planned_admissions()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.FormDesign);
        await desk.Page.GetByRole(AriaRole.Group, new() { Name = "Transfer notes textbox", Exact = true }).PressAsync("Enter");
        await desk.Page.GetByRole(AriaRole.Tab, new() { Name = "Conditions", Exact = true }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Button, new() { Name = "Edit Conditional Visibility" }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Value" }).FillAsync("Planned");
        await desk.Page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Value" }).PressAsync("Tab");
        await desk.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Link, new() { Name = "Reopen device draft" }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Button, new() { Name = "Preview form", Exact = true }).PressAsync("Enter");
        var route = desk.Page.GetByRole(AriaRole.Textbox, new() { Name = "Arrival route textbox", Exact = true });
        var notes = desk.Page.GetByRole(AriaRole.Textbox, new() { Name = "Transfer notes textbox", Exact = true });
        await Expect(route).ToHaveValueAsync("Hospital");
        await Expect(notes).ToBeHiddenAsync();
        await route.FillAsync("Planned");
        await route.PressAsync("Tab");
        await Expect(notes).ToBeVisibleAsync();
        await route.FillAsync("Respite");
        await route.PressAsync("Tab");
        await Expect(notes).ToBeHiddenAsync();
    }
}
