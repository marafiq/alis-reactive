namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.CrossVendor;

[TestFixture]
public class WhenComponentApiExercisedEndToEnd : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/TestWidget";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task page_loads_with_no_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("TestWidget — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── 1: Property Write (static) ──

    [Test]
    public async Task property_write_sets_value_on_dom_ready()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#tw-write input"))
            .ToHaveValueAsync("initialized", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 2: Void Method — Clear ──

    [Test]
    public async Task clear_empties_the_widget()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#tw-clear input")).ToHaveValueAsync("clear-me");
        await Page.Locator("#btn-clear").ClickAsync();
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 3: Void Method — Focus ──

    [Test]
    public async Task focus_gives_inner_input_focus()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-focus").ClickAsync();
        await Expect(Page.Locator("#tw-focus input"))
            .ToBeFocusedAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 4: Event Payload → Component SetValue ──

    [Test]
    public async Task event_payload_writes_value_to_mirror_widget()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-event-write input").FillAsync("hello");
        await Expect(Page.Locator("#tw-event-mirror input"))
            .ToHaveValueAsync("hello", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 5: Component Read → Display ──

    [Test]
    public async Task component_read_displays_value()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#read-echo"))
            .ToHaveTextAsync("read-me", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 6: Component Value Condition ──

    [Test]
    public async Task condition_shows_indicator_when_not_empty()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#comp-cond-ind")).ToBeHiddenAsync();
        await Page.Locator("#tw-comp-cond input").FillAsync("x");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_hides_indicator_when_empty()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-comp-cond input").FillAsync("x");
        await Expect(Page.Locator("#comp-cond-ind")).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Page.Locator("#tw-comp-cond input").FillAsync("");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 7: Method + Arg (SetItems from event) ──

    [Test]
    public async Task setitems_from_event_payload()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-load-items").ClickAsync();
        // setItems renders <li> elements inside the widget
        await Expect(Page.Locator("#tw-items .test-widget-item"))
            .ToHaveCountAsync(3, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 8: Cross-Component Read → Write ──

    [Test]
    public async Task cross_component_read_writes_to_target()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-source input").FillAsync("synced");
        await Expect(Page.Locator("#tw-target input"))
            .ToHaveValueAsync("synced", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 9: Gather → HTTP Echo ──

    [Test]
    public async Task gather_posts_widget_value()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-gather").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 10: Response Body → Component Property Write ──

    [Test]
    public async Task response_body_sets_widget_value()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-datasource").ClickAsync();
        await Expect(Page.Locator("#tw-ds-target input"))
            .ToHaveValueAsync("beta", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 11: Response Body → Component Method (SetItems) ──

    [Test]
    public async Task response_body_sets_widget_items()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-datasource").ClickAsync();
        // setItems renders <li> elements: alpha, beta, gamma
        await Expect(Page.Locator("#tw-ds-target .test-widget-item"))
            .ToHaveCountAsync(3, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ══════════════════════════════════════════════════════════
    // Deep BDD Regression Scenarios — Multi-Step Workflows
    // ══════════════════════════════════════════════════════════

    // ── State Management: clear resets, then user input re-establishes state ──

    [Test]
    public async Task setting_value_then_clearing_then_setting_again_proves_state_management()
    {
        // Proves the component state resets properly and accepts new input after clear.
        // Initial value "clear-me" → clear button resets → user types new value → value sticks.
        await NavigateAndBoot();

        // Step 1: verify initial state from server-rendered initial value
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("clear-me", new() { Timeout = 3000 });

        // Step 2: clear resets the value completely
        await Page.Locator("#btn-clear").ClickAsync();
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });

        // Step 3: user types a new value — component accepts input after being cleared
        await Page.Locator("#tw-clear input").FillAsync("resurrected");
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("resurrected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Event Payload → Condition Guard → Then/Else DOM Mutation ──

    [Test]
    public async Task event_payload_flows_through_condition_to_dom_mutation()
    {
        // Proves the full chain: component change event → condition evaluates
        // component value → NotEmpty guard → Then branch shows indicator with text,
        // Else branch hides indicator. Three-step toggle proves both branches.
        await NavigateAndBoot();

        // Step 1: indicator starts hidden (no value in component)
        await Expect(Page.Locator("#comp-cond-ind")).ToBeHiddenAsync();

        // Step 2: type a value → change event fires → condition evaluates NotEmpty → Then branch
        await Page.Locator("#tw-comp-cond input").FillAsync("trigger-then");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToHaveTextAsync("has value", new() { Timeout = 3000 });

        // Step 3: clear the value → change event fires → condition evaluates empty → Else branch
        await Page.Locator("#tw-comp-cond input").FillAsync("");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        // Step 4: type a different value → proves condition re-evaluates correctly on second pass
        await Page.Locator("#tw-comp-cond input").FillAsync("second-trigger");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToHaveTextAsync("has value", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Cross-Component Sync: multiple value changes propagate correctly ──

    [Test]
    public async Task cross_component_sync_tracks_multiple_value_changes()
    {
        // Proves source→target sync works across sequential value changes,
        // not just the first write. Each new source value replaces the previous target value.
        await NavigateAndBoot();

        // Step 1: first value syncs
        await Page.Locator("#tw-source input").FillAsync("alpha");
        await Expect(Page.Locator("#tw-target input"))
            .ToHaveValueAsync("alpha", new() { Timeout = 3000 });

        // Step 2: second value replaces the first
        await Page.Locator("#tw-source input").FillAsync("bravo");
        await Expect(Page.Locator("#tw-target input"))
            .ToHaveValueAsync("bravo", new() { Timeout = 3000 });

        // Step 3: clearing source propagates empty to target
        await Page.Locator("#tw-source input").FillAsync("");
        await Expect(Page.Locator("#tw-target input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Event Payload Mirror: multiple writes prove event system is stateless ──

    [Test]
    public async Task event_payload_mirror_reflects_successive_value_changes()
    {
        // Proves that each change event independently carries the new value via payload,
        // not a cached reference. Three successive values must each arrive at the mirror.
        await NavigateAndBoot();

        await Page.Locator("#tw-event-write input").FillAsync("first");
        await Expect(Page.Locator("#tw-event-mirror input"))
            .ToHaveValueAsync("first", new() { Timeout = 3000 });

        await Page.Locator("#tw-event-write input").FillAsync("second");
        await Expect(Page.Locator("#tw-event-mirror input"))
            .ToHaveValueAsync("second", new() { Timeout = 3000 });

        await Page.Locator("#tw-event-write input").FillAsync("third");
        await Expect(Page.Locator("#tw-event-mirror input"))
            .ToHaveValueAsync("third", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Response Body: property write AND method call arrive in single HTTP roundtrip ──

    [Test]
    public async Task response_body_delivers_both_value_and_items_in_single_roundtrip()
    {
        // Proves that a single HTTP response can drive both a property write (SetValue)
        // and a method call (SetItems) on the same component — the runtime executes
        // all response commands sequentially without dropping any.
        await NavigateAndBoot();

        // Before fetch — no value, no items
        await Expect(Page.Locator("#tw-ds-target input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });
        await Expect(Page.Locator("#tw-ds-target .test-widget-item"))
            .ToHaveCountAsync(0, new() { Timeout = 3000 });

        // Single click triggers GET → response body drives both mutations
        await Page.Locator("#btn-datasource").ClickAsync();

        // Both arrive from the same response
        await Expect(Page.Locator("#tw-ds-target input"))
            .ToHaveValueAsync("beta", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tw-ds-target .test-widget-item"))
            .ToHaveCountAsync(3, new() { Timeout = 5000 });

        // Verify the actual item content matches the server response
        var items = Page.Locator("#tw-ds-target .test-widget-item");
        await Expect(items.Nth(0)).ToHaveTextAsync("alpha");
        await Expect(items.Nth(1)).ToHaveTextAsync("beta");
        await Expect(items.Nth(2)).ToHaveTextAsync("gamma");

        AssertNoConsoleErrors();
    }

    // ── Clear also resets items, proving void method affects full component state ──

    [Test]
    public async Task clear_resets_both_value_and_focus_state()
    {
        // Proves that the clear void method resets the component value AND that
        // the focus void method can be called on the same cleared component.
        // Section 2 wires: Clear().Focus() — both void methods chain.
        await NavigateAndBoot();

        // Initial state has a value
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("clear-me", new() { Timeout = 3000 });

        // Clear + Focus fires as a chain
        await Page.Locator("#btn-clear").ClickAsync();

        // Value is cleared
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });

        // Focus was also applied (the Clear button handler chains Clear().Focus())
        await Expect(Page.Locator("#tw-clear input"))
            .ToBeFocusedAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Condition guard text content proves Then branch executes mutations ──

    [Test]
    public async Task condition_then_branch_sets_text_and_shows_indicator()
    {
        // Proves that the Then branch executes BOTH mutations: Show() AND SetText("has value").
        // The indicator must be visible AND contain the correct text — partial execution would fail.
        await NavigateAndBoot();

        await Expect(Page.Locator("#comp-cond-ind")).ToBeHiddenAsync();

        await Page.Locator("#tw-comp-cond input").FillAsync("any-value");

        // Both mutations from the Then branch must have executed
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToHaveTextAsync("has value", new() { Timeout = 3000 });

        // Now clear — Else branch hides
        await Page.Locator("#tw-comp-cond input").FillAsync("");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        // Re-enter a value — Then branch must set the text again (not stale from first pass)
        await Page.Locator("#tw-comp-cond input").FillAsync("another-value");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToHaveTextAsync("has value", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── SetItems: item content matches dispatched payload ──

    [Test]
    public async Task setitems_renders_correct_item_content_from_payload()
    {
        // Proves that setItems not only renders the correct count but also
        // the correct content — each <li> text matches the dispatched array.
        await NavigateAndBoot();

        await Page.Locator("#btn-load-items").ClickAsync();

        var items = Page.Locator("#tw-items .test-widget-item");
        await Expect(items).ToHaveCountAsync(3, new() { Timeout = 5000 });

        // The dispatched payload is ["x", "y", "z"]
        await Expect(items.Nth(0)).ToHaveTextAsync("x");
        await Expect(items.Nth(1)).ToHaveTextAsync("y");
        await Expect(items.Nth(2)).ToHaveTextAsync("z");

        AssertNoConsoleErrors();
    }

    // ── DomReady property write does not interfere with subsequent user input ──

    [Test]
    public async Task dom_ready_property_write_does_not_block_subsequent_user_input()
    {
        // Proves that after DomReady sets "initialized" on tw-write, the component
        // still accepts user input. The plan-driven write does not lock the component.
        await NavigateAndBoot();

        // DomReady has already set the value
        await Expect(Page.Locator("#tw-write input"))
            .ToHaveValueAsync("initialized", new() { Timeout = 3000 });

        // User can still type — component is not locked by the plan-driven write
        await Page.Locator("#tw-write input").FillAsync("user-override");
        await Expect(Page.Locator("#tw-write input"))
            .ToHaveValueAsync("user-override", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
