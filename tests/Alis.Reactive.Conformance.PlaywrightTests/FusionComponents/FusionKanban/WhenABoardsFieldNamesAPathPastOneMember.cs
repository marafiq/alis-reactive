using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABoardsFieldNamesAPathPastOneMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F26.M21_X1055_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.NursedRoundByNurse);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionKanbanFields names 'c => c.Nurse.Name', a path past one member of the card, "
                + "and the board reads each field as one member of a card; name one member, such as card => card.Status.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.NursedRoundByMedicine);
        await Expect(rendered.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Aspirin", Exact = true })).ToBeVisibleAsync();
    }
}
