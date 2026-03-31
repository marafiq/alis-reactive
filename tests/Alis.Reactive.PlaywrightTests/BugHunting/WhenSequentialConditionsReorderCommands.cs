namespace Alis.Reactive.PlaywrightTests.BugHunting;

/// <summary>
/// Proves that FlushSegment bundles commands between two When/Then blocks
/// with commands before the first When — losing their intended execution order.
///
/// The DSL pipeline:
///   p.Element("step1").SetText("1-before");     // before cond1
///   p.When(...).Eq("high").Then(...).Else(...);  // cond1
///   p.Element("step3").SetText("3-between");     // between cond1 and cond2
///   p.When(...).Eq("low").Then(...).Else(...);   // cond2
///   p.Element("step5").SetText("5-after");       // after cond2
///
/// Expected serialization: 5 entries in declaration order:
///   1. Sequential([step1])
///   2. Conditional(cond1 branches)
///   3. Sequential([step3])
///   4. Conditional(cond2 branches)
///   5. Sequential([step5])
///
/// Actual serialization (bug): step1 and step3 are bundled into one sequential,
/// both running BEFORE cond1:
///   1. Sequential([step1, step3])  ← step3 moved before cond1
///   2. Conditional(cond1 branches)
///   3. Sequential([step5])
///   4. Conditional(cond2 branches)
///
/// Root cause: PipelineBuilder.FlushSegment() at line 224-229 flushes
/// ALL accumulated Commands as one SequentialReaction, regardless of
/// whether some were added before vs after the current conditional block.
///
/// Page under test: /Sandbox/Conditions/SequentialConditions
/// </summary>
[TestFixture]
public class WhenSequentialConditionsReorderCommands : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/SequentialConditions";

    [Test]
    public async Task step3_between_conditions_should_not_be_bundled_with_step1()
    {
        // The plan JSON proves the bug at serialization time.
        // step1 and step3 are in the same sequential entry.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Read the serialized plan JSON
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        // The first entry should contain ONLY step1, not step3.
        // If step3 is in the first entry, the bug is confirmed.
        Assert.That(planJson, Does.Contain("\"target\": \"step1\""),
            "Plan must contain step1 command");
        Assert.That(planJson, Does.Contain("\"target\": \"step3\""),
            "Plan must contain step3 command");

        // Parse entries to check ordering
        var firstEntryHasStep3 = planJson!.IndexOf("\"target\": \"step3\"") <
                                  planJson.IndexOf("\"kind\": \"conditional\"");

        if (firstEntryHasStep3)
        {
            TestContext.Out.WriteLine(
                "[BUG CONFIRMED] step3 (between conditions) appears BEFORE the first " +
                "conditional entry in the serialized plan. FlushSegment bundles all " +
                "accumulated commands together regardless of condition boundaries.");
        }

        // step3 should appear AFTER the first conditional, not before it
        Assert.That(firstEntryHasStep3, Is.False,
            "step3 (between cond1 and cond2) must be serialized AFTER the first " +
            "conditional, not bundled with step1 before it");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_commands_execute_but_step3_runs_before_cond1_instead_of_after()
    {
        // Even though the ordering is wrong, all commands DO execute.
        // This test verifies all outputs are populated after selecting a value.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var dropdown = Page.Locator(
            "#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_SequentialConditions_SequentialConditionsModel__Value");
        await dropdown.SelectOptionAsync("high");

        // All 5 outputs should be populated
        await Expect(Page.Locator("#step1"))
            .ToHaveTextAsync("1-before", new() { Timeout = 3000 });
        await Expect(Page.Locator("#cond1-result"))
            .ToHaveTextAsync("cond1-high", new() { Timeout = 3000 });
        await Expect(Page.Locator("#step3"))
            .ToHaveTextAsync("3-between", new() { Timeout = 3000 });
        await Expect(Page.Locator("#cond2-result"))
            .ToHaveTextAsync("cond2-not-low", new() { Timeout = 3000 });
        await Expect(Page.Locator("#step5"))
            .ToHaveTextAsync("5-after", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
