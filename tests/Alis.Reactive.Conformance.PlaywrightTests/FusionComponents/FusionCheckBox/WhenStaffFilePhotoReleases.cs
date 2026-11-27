using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffFilePhotoReleases(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F10.M21_X1309_0)]
    public async Task the_selected_filing_code_reaches_the_native_form_receipt()
    {
        var clinical = await NewWorld();
        await clinical.Page.GotoAsync(CarePages.PhotoReleaseFiling);
        await clinical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use clinical filing", Exact = true }).ClickAsync();
        await clinical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review filing destination", Exact = true }).ClickAsync();
        await Expect(clinical.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing destination", Exact = true })).ToHaveTextAsync("Clinical record");
        await clinical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File release", Exact = true }).ClickAsync();
        await Expect(clinical.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Release receipt", Exact = true })).ToHaveTextAsync("Release filed in the clinical record");

        var directory = await NewWorld();
        await directory.Page.GotoAsync(CarePages.PhotoReleaseClinical);
        await directory.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory filing", Exact = true }).ClickAsync();
        await directory.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review filing destination", Exact = true }).ClickAsync();
        await Expect(directory.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing destination", Exact = true })).ToHaveTextAsync("Resident directory");
        await directory.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File release", Exact = true }).ClickAsync();
        await Expect(directory.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Release receipt", Exact = true })).ToHaveTextAsync("Release filed in the resident directory");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1310_0)]
    public async Task the_signed_copy_uses_the_inputs_custom_or_standard_notice()
    {
        var custom = await NewWorld();
        await custom.Page.GotoAsync(CarePages.PhotoReleaseDirectory);
        await custom.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signed copy", Exact = true }).ClickAsync();
        await Expect(custom.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signed copy notice", Exact = true })).ToHaveTextAsync("Permission for a directory photo");

        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.PhotoReleaseFiling);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signed copy", Exact = true }).ClickAsync();
        await Expect(standard.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signed copy notice", Exact = true })).ToHaveTextAsync("Photo permission");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1311_0)]
    public async Task the_spoken_filing_instruction_uses_the_rendered_controls_accessible_notice()
    {
        var custom = await NewWorld();
        await custom.Page.GotoAsync(CarePages.PhotoReleaseDirectory);
        await custom.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review spoken instruction", Exact = true }).ClickAsync();
        await Expect(custom.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Spoken instruction", Exact = true })).ToHaveTextAsync("Permission for a directory photo");

        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.PhotoReleaseFiling);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review spoken instruction", Exact = true }).ClickAsync();
        await Expect(standard.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Spoken instruction", Exact = true })).ToHaveTextAsync("Photo permission");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1314_0)]
    public async Task filing_distinguishes_an_uncategorized_release_from_no_signed_release()
    {
        var uncategorized = await NewWorld();
        await uncategorized.Page.GotoAsync(CarePages.PhotoReleaseFiling);
        await uncategorized.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear filing code", Exact = true }).ClickAsync();
        await uncategorized.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File release", Exact = true }).ClickAsync();
        await Expect(uncategorized.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Release receipt", Exact = true })).ToHaveTextAsync("Release filed under general consent");

        var uncheckedRelease = await NewWorld();
        await uncheckedRelease.Page.GotoAsync(CarePages.PhotoReleaseFiling);
        await uncheckedRelease.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear filing code", Exact = true }).ClickAsync();
        await uncheckedRelease.Page.GetByText("Photo permission", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await uncheckedRelease.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File release", Exact = true }).ClickAsync();
        await Expect(uncheckedRelease.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Release receipt", Exact = true })).ToHaveTextAsync("No photo release filed");
    }
}
