using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffReviseACareNote(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F48.M21_X2010_0)]
    public async Task a_locked_care_note_accepts_a_correction_only_after_it_is_reopened()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.ProfileNoteWritten);
        var note = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care note", Exact = true }).GetByRole(AriaRole.Textbox);

        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock care note", Exact = true }).ClickAsync();
        await note.PressAsync("End");
        await note.PressSequentiallyAsync(" with milk");
        await Expect(note).ToHaveValueAsync("Prefers tea at seven");

        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Revise care note", Exact = true }).ClickAsync();
        await note.PressAsync("End");
        await note.PressSequentiallyAsync(" with milk");
        await Expect(note).ToHaveValueAsync("Prefers tea at seven with milk");
    }
}
