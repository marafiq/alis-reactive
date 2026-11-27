using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenWardStaffFileStocktakes(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1605_0)]
    public async Task filing_the_stocktake_distinguishes_an_empty_cart_from_an_outstanding_count()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.LinenStocktake);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File stocktake", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Ward stocktake filed with 0 towels");

        var pending = await NewWorld();
        await pending.Page.GotoAsync(CarePages.LinenStocktake);
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File stocktake", Exact = true }).ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Ward stocktake filed awaiting a count");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1606_0)]
    public async Task the_stock_log_distinguishes_a_staff_count_from_a_book_correction()
    {
        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.LinenStocktake);
        await staff.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock change", Exact = true })).ToHaveTextAsync("Staff counted one extra towel");

        var book = await NewWorld();
        await book.Page.GotoAsync(CarePages.LinenStocktake);
        await book.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply book correction", Exact = true }).ClickAsync();
        await Expect(book.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock change", Exact = true })).ToHaveTextAsync("Book correction added two towels");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1607_0)]
    public async Task recount_and_review_handoffs_move_focus_to_and_from_the_tally()
    {
        var count = await NewWorld();
        await count.Page.GotoAsync(CarePages.LinenStocktake);
        await count.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Begin recount", Exact = true }).ClickAsync();
        await Expect(count.Page.GetByRole(AriaRole.Spinbutton)).ToBeFocusedAsync();
        await Expect(count.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Counting the ward trolley");

        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.LinenStocktake);
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send tally for review", Exact = true }).ClickAsync();
        await Expect(review.Page.GetByRole(AriaRole.Spinbutton)).Not.ToBeFocusedAsync();
        await Expect(review.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Ward tally ready for review");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1611_0)]
    public async Task a_sealed_count_is_read_only_and_a_fresh_count_accepts_a_keyboard_step()
    {
        var sealedCount = await NewWorld();
        await sealedCount.Page.GotoAsync(CarePages.LinenStocktake);
        await sealedCount.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Seal audited count", Exact = true }).ClickAsync();
        await Expect(sealedCount.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("aria-readonly", "true");
        await Expect(sealedCount.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Count custody", Exact = true })).ToHaveTextAsync("Audited count is locked");

        var opened = await NewWorld();
        await opened.Page.GotoAsync(CarePages.LinenStocktake);
        await opened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Seal audited count", Exact = true }).ClickAsync();
        await opened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open fresh count", Exact = true }).ClickAsync();
        await opened.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await Expect(opened.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");
        await Expect(opened.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Count custody", Exact = true })).ToHaveTextAsync("Ward staff can count");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1612_0)]
    public async Task bedside_and_trolley_counting_offer_the_tools_staff_use_at_each_station()
    {
        var bedside = await NewWorld();
        await bedside.Page.GotoAsync(CarePages.LinenStocktake);
        await bedside.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Bedside counting tools", Exact = true }).ClickAsync();
        await Expect(bedside.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Counting instructions", Exact = true })).ToHaveTextAsync("Clear a count beside the bed");
        await bedside.Page.GetByRole(AriaRole.Spinbutton).ClickAsync();
        await bedside.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true }).ClickAsync();
        await Expect(bedside.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("");

        var trolley = await NewWorld();
        await trolley.Page.GotoAsync(CarePages.LinenStocktake);
        await trolley.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Bedside counting tools", Exact = true }).ClickAsync();
        await trolley.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Trolley counting tools", Exact = true }).ClickAsync();
        await Expect(trolley.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Counting instructions", Exact = true })).ToHaveTextAsync("Step the count on the trolley");
        await trolley.Page.GetByLabel("Increment value", new PageGetByLabelOptions { Exact = true }).ClickAsync();
        await Expect(trolley.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1613_0)]
    public async Task ward_staff_can_reopen_a_closed_count()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.LinenStocktake);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close ward count", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("disabled", "");
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ward count", Exact = true })).ToHaveTextAsync("Ward count closed");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LinenStocktake);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close ward count", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reopen ward count", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await Expect(open.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ward count", Exact = true })).ToHaveTextAsync("Ward count reopened");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1614_0)]
    public async Task floor_and_receiving_staff_review_the_instruction_on_their_own_control()
    {
        var floor = await NewWorld();
        await floor.Page.GotoAsync(CarePages.LinenStocktake);
        await floor.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review spoken instruction", Exact = true }).ClickAsync();
        await Expect(floor.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Spoken instruction", Exact = true })).ToHaveTextAsync("Count towels on the ward floor");

        var receiving = await NewWorld();
        await receiving.Page.GotoAsync(CarePages.LinenReceiving);
        await receiving.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review spoken instruction", Exact = true }).ClickAsync();
        await Expect(receiving.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Spoken instruction", Exact = true })).ToHaveTextAsync("Count towels at receiving");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1615_0)]
    public async Task kept_and_one_visit_stocktakes_reopen_with_their_respective_counts()
    {
        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.LinenStocktake);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep trolley worksheet", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await kept.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Reopen worksheet", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");

        var visit = await NewWorld();
        await visit.Page.GotoAsync(CarePages.LinenStocktake);
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use one-visit worksheet", Exact = true }).ClickAsync();
        await visit.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await visit.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Reopen worksheet", Exact = true }).ClickAsync();
        await Expect(visit.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("8");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1616_0)]
    public async Task recovery_receipts_distinguish_empty_and_outstanding_stocktakes()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.LinenStocktake);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive stocktake", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Recovery receipt", Exact = true })).ToHaveTextAsync("Archived empty cart");

        var pending = await NewWorld();
        await pending.Page.GotoAsync(CarePages.LinenStocktake);
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive stocktake", Exact = true }).ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Recovery receipt", Exact = true })).ToHaveTextAsync("Archived outstanding count");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1617_0)]
    public async Task a_paper_tally_releases_the_enhanced_counter_and_an_electronic_tally_keeps_it()
    {
        var paper = await NewWorld();
        await paper.Page.GotoAsync(CarePages.LinenStocktake);
        await paper.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release to paper tally", Exact = true }).ClickAsync();
        await Expect(paper.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally guide", Exact = true })).ToHaveTextAsync("Paper tally opened");
        var transcription = paper.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Count towels on the ward floor", Exact = true });
        await transcription.FillAsync("Two bundles and three loose towels");
        await Expect(transcription).ToBeFocusedAsync();
        await Expect(paper.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign stocktake return", Exact = true })).ToBeHiddenAsync();

        var electronic = await NewWorld();
        await electronic.Page.GotoAsync(CarePages.LinenStocktake);
        await electronic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reopen electronic tally", Exact = true }).ClickAsync();
        await Expect(electronic.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally guide", Exact = true })).ToHaveTextAsync("Electronic tally open");
        await Expect(electronic.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("8");
        await Expect(electronic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign stocktake return", Exact = true })).ToBeVisibleAsync();
    }
}
