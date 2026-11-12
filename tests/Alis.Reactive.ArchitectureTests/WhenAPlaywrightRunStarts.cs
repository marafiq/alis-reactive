using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAPlaywrightRunStarts
{
    private const string Landings = "spec/tools/landings.py";
    private const string Selector = "scripts/playwright-select.mjs";
    private const string Residents = "Tests.Residents.List.WhenAResidentIsListed";
    private const string Rounds = "Tests.Rounds.Round.WhenARoundStarts";
    private const string Worlds = "Tests.Harness.WhenTwoWorldsRun";

    private static readonly string[] TheSpec =
    [
        "M1.F1 X1.1 0\tM1_F1.X1_1_0\tU3\tresidents\tnet10\tpool",
        "M1.F1 X1.2 0\tM1_F1.X1_2_0\tU4\tresidents\tnet10\tpool",
        "M1.F1 X1.2 0\tM1_F1.X1_2_0\tU4\tresidents\tnet48\ttwin",
        "M2.F1 X2.1 0\tM2_F1.X2_1_0\tU4\tround\tsnakeCase\tpool",
        "M2.F1 X2.2 0\tM2_F1.X2_2_0\tU5\tround\tnet48\ttwin",
        "harness WhenTwoWorldsRun.each_world_lists_its_own\tHarness.WhenTwoWorldsRun_each_world_lists_its_own\tU2\t\tnet10\tpool",
    ];

    private static readonly string[] TheAssembly =
    [
        $"M1.F1 X1.1 0\t{Residents}\tthe_resident_is_listed",
        $"M1.F1 X1.2 0\t{Residents}\tthe_resident_is_opened",
        $"M2.F1 X2.1 0\t{Rounds}\tthe_round_is_listed",
        $"M2.F1 X2.2 0\t{Rounds}\tthe_round_is_signed",
        $"harness WhenTwoWorldsRun.each_world_lists_its_own\t{Worlds}\teach_world_lists_its_own",
    ];

    private const string Grid = "Tests.FusionComponents.FusionGrid.Residents.WhenAGridSorts";
    private const string MultiSelect = "Tests.FusionComponents.FusionMultiSelect.Bench.WhenAnItemIsPicked";
    private const string Badge = "Tests.FusionTemplates.Residents.WhenABadgeRenders";

    private static readonly string[] TheSpecWithFusion =
    [
        .. TheSpec,
        "M1.F1 X1.1 0\tM1_F1.X1_1_0\tU3\tresidents\tsnakeCase\tpool",
        "M9.F1 X9.1 0\tM9_F1.X9_1_0\tU21\tresidents\tnet10\tpool",
        "M21.F3 M21.X1 0\tM21_F3.M21_X1_0\tU26\tresidents\tnet10\tpool",
        "M21.F3 M21.X1 0\tM21_F3.M21_X1_0\tU26\tresidents\tfusionShell\tpool",
        "M21.F2 M21.X2 0\tM21_F2.M21_X2_0\tU25\tbench\tnet10\tpool",
        "M22.F1 M22.X1 0\tM22_F1.M22_X1_0\tU25\tresidents\tnet10\tpool",
        "M22.F1 M22.X1 0\tM22_F1.M22_X1_0\tU25\tresidents\tnet48\ttwin",
    ];

    private static readonly string[] TheAssemblyWithFusion =
    [
        .. TheAssembly,
        $"M21.F3 M21.X1 0\t{Grid}\tthe_grid_sorts",
        $"M21.F2 M21.X2 0\t{MultiSelect}\tthe_item_is_picked",
        $"M22.F1 M22.X1 0\t{Badge}\tthe_badge_renders",
    ];

    [Test]
    public void every_test_of_the_spec_lands_in_one_unit_under_a_host_of_one_kind()
    {
        var landed = Run("python3", [Path.Combine(Repository.Root, Landings)]);
        Assert.That(landed.ExitCode, Is.Zero, landed.Errors);
        var rows = landed.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split('\t')).ToList();

        Assert.That(rows, Is.Not.Empty, Landings + " printed no test of the spec");
        var misshapen = rows.Where(row => row.Length != 6 || !Regex.IsMatch(row[2], "^U[1-9][0-9]*$") || row[4].Length == 0 || row[5] is not ("pool" or "twin" or "fresh"))
            .Select(row => string.Join(" | ", row)).ToList();
        Assert.That(misshapen, Is.Empty, "A test of the spec is landed with no unit, no host or no kind of host:\n" + string.Join("\n", misshapen));
        var landedTwice = rows.GroupBy(row => row[0]).Where(test => test.Select(row => row[1] + " " + row[2]).Distinct().Count() > 1).Select(test => test.Key).ToList();
        Assert.That(landedTwice, Is.Empty, "A test of the spec lands in two units or under two constants:\n" + string.Join("\n", landedTwice));
    }

    [AcceptedCase]
    public void a_unit_takes_the_tests_the_spec_lands_in_it_under_the_hosts_served_here()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly, "--unit", "U4");

        Assert.That(selected.Errors, Is.Empty);
        Assert.That(selected.Cases, Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_opened", $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
        Assert.That(selected.Filter, Is.EqualTo(
            $"((FullyQualifiedName~{Residents}&Name=the_resident_is_opened)|(FullyQualifiedName~{Rounds}&Name=the_round_is_listed))&TestCategory!=net48"));
        Assert.That(selected.Label, Is.EqualTo("U4"));
    }

    [AcceptedCase]
    public void every_test_of_the_assembly_is_taken_with_no_filter_but_the_hosts_served_elsewhere()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly, "--all");

        Assert.That(selected.Cases, Is.EqualTo(new[]
        {
            $"{Worlds}(\"net10\").each_world_lists_its_own",
            $"{Residents}(\"net10\").the_resident_is_listed",
            $"{Residents}(\"net10\").the_resident_is_opened",
            $"{Rounds}(\"snakeCase\").the_round_is_listed",
        }));
        Assert.That(selected.Filter, Is.EqualTo($"(FullyQualifiedName~{Worlds}|FullyQualifiedName~{Residents}|(FullyQualifiedName~{Rounds}&Name=the_round_is_listed))&TestCategory!=net48"));
        Assert.That(selected.Label, Is.EqualTo("all"));
    }

    [AcceptedCase]
    public void through_a_unit_takes_that_unit_and_every_unit_before_it()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly, "--through", "U3");

        Assert.That(selected.Cases, Is.EqualTo(new[] { $"{Worlds}(\"net10\").each_world_lists_its_own", $"{Residents}(\"net10\").the_resident_is_listed" }));
        Assert.That(selected.Label, Is.EqualTo("through U3"));
    }

    [AcceptedCase]
    public void a_feature_a_screen_a_constant_and_a_name_each_take_their_tests()
    {
        using var written = new WrittenFolder();

        string[] withALongerFeature = [.. TheSpec, "M1.F10 X1.1 0\tM1_F10.X1_1_0\tU7\tresidents\tnet10\tpool"];
        string[] withItsTest = [.. TheAssembly, $"M1.F10 X1.1 0\t{Residents}\tthe_resident_is_moved"];

        Assert.That(Select(written, withALongerFeature, withItsTest, "--feature", "M1.F1").Cases,
            Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_listed", $"{Residents}(\"net10\").the_resident_is_opened" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "--screen", "round").Cases, Is.EqualTo(new[] { $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "--test", "M1_F1.X1_1_0").Cases, Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_listed" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "Harness").Cases, Is.EqualTo(new[] { $"{Worlds}(\"net10\").each_world_lists_its_own" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "WhenARoundStarts").Cases, Is.EqualTo(new[] { $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
    }

    [AcceptedCase]
    public void a_fixture_whose_name_opens_another_fixtures_name_is_taken_without_the_other()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpec, "M2.F1 X2.3 0\tM2_F1.X2_3_0\tU6\tround\tsnakeCase\tpool"];
        string[] assembly = [.. TheAssembly, $"M2.F1 X2.3 0\t{Rounds}Late\tthe_late_round_is_listed"];

        var one = Select(written, spec, assembly, "--test", "M2_F1.X2_1_0");
        var whole = Select(written, spec, assembly, "--test", "M1_F1.X1_1_0");
        var longer = Select(written, spec, assembly, "WhenARoundStartsLate");

        Assert.That(one.Filter, Is.EqualTo($"(FullyQualifiedName~{Rounds}&FullyQualifiedName!~{Rounds}Late&Name=the_round_is_listed)"));
        Assert.That(whole.Filter, Is.EqualTo($"(FullyQualifiedName~{Residents}&Name=the_resident_is_listed)"));
        Assert.That(longer.Filter, Is.EqualTo($"FullyQualifiedName~{Rounds}Late"));
        Assert.That(longer.Cases, Is.EqualTo(new[] { $"{Rounds}Late(\"snakeCase\").the_late_round_is_listed" }));
    }

    [AcceptedCase]
    public void a_host_takes_that_host_alone_whoever_serves_it()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly, "--unit", "U4", "--host", "net48");

        Assert.That(selected.Cases, Is.EqualTo(new[] { $"{Residents}(\"net48\").the_resident_is_opened" }));
        Assert.That(selected.Units, Is.EqualTo(new[] { "twin" }));
        Assert.That(selected.Filter, Is.EqualTo($"(FullyQualifiedName~{Residents}&Name=the_resident_is_opened)&TestCategory=net48"));
        Assert.That(selected.Label, Is.EqualTo("U4@net48"));
    }

    [AcceptedCase]
    public void a_pool_takes_its_tests_of_the_assembly_under_the_hosts_a_pool_serves()
    {
        using var written = new WrittenFolder();

        var core = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "core");
        var hosts = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "hosts");
        var fusion = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "fusion");

        Assert.That(core.Cases, Is.EqualTo(new[]
        {
            $"{Worlds}(\"net10\").each_world_lists_its_own",
            $"{Residents}(\"net10\").the_resident_is_listed",
            $"{Residents}(\"net10\").the_resident_is_opened",
        }));
        Assert.That(core.Filter, Is.EqualTo($"(FullyQualifiedName~{Worlds}|FullyQualifiedName~{Residents})&TestCategory!=fusionShell&TestCategory!=net48&TestCategory!=snakeCase"));
        Assert.That(hosts.Cases, Is.EqualTo(new[] { $"{Residents}(\"snakeCase\").the_resident_is_listed", $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
        Assert.That(hosts.Units.Distinct(), Is.EqualTo(new[] { "hosts" }));
        Assert.That(fusion.Cases, Is.EqualTo(new[]
        {
            $"{Grid}(\"fusionShell\").the_grid_sorts",
            $"{Grid}(\"net10\").the_grid_sorts",
            $"{MultiSelect}(\"net10\").the_item_is_picked",
            $"{Badge}(\"net10\").the_badge_renders",
        }));
        Assert.That(fusion.Filter, Is.EqualTo($"(FullyQualifiedName~{Grid}|FullyQualifiedName~{MultiSelect}|FullyQualifiedName~{Badge})&TestCategory!=net48&TestCategory!=snakeCase"));
        Assert.That(new[] { core.Label, hosts.Label, fusion.Label, fusion.Shard }, Is.EqualTo(new[] { "core", "hosts", "fusion", "1/1" }));
    }

    [AcceptedCase]
    public void a_shard_takes_its_units_longest_first_each_onto_the_lightest_shard()
    {
        using var written = new WrittenFolder();
        const string weights = "{ \"FusionComponents.FusionGrid\": 10, \"FusionTemplates\": 25, \"FusionComponents.FusionMultiSelect\": 30, \"Residents\": 90 }";

        var first = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, weights, "--pool", "fusion", "--shard", "1/2");
        var second = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, weights, "--pool", "fusion", "--shard", "2/2");

        Assert.That(first.Cases, Is.EqualTo(new[] { $"{MultiSelect}(\"net10\").the_item_is_picked" }));
        Assert.That(first.Units, Is.EqualTo(new[] { "FusionComponents.FusionMultiSelect" }));
        Assert.That(first.Filter, Is.EqualTo($"FullyQualifiedName~{MultiSelect}&TestCategory!=fusionShell&TestCategory!=net48&TestCategory!=snakeCase"));
        Assert.That(second.Cases, Is.EqualTo(new[] { $"{Grid}(\"fusionShell\").the_grid_sorts", $"{Grid}(\"net10\").the_grid_sorts", $"{Badge}(\"net10\").the_badge_renders" }));
        Assert.That(second.Units, Is.EqualTo(new[] { "FusionComponents.FusionGrid", "FusionComponents.FusionGrid", "FusionTemplates" }));
        Assert.That(second.Filter, Is.EqualTo($"(FullyQualifiedName~{Grid}|FullyQualifiedName~{Badge})&TestCategory!=net48&TestCategory!=snakeCase"));
        Assert.That(new[] { first.Shard, second.Shard }, Is.EqualTo(new[] { "1/2", "2/2" }));
    }

    [AcceptedCase]
    public void a_unit_with_no_weight_is_placed_last_on_the_lightest_shard()
    {
        using var written = new WrittenFolder();
        const string stale = "{ \"FusionComponents.FusionGrid\": 30, \"FusionTemplates\": 25 }";

        var first = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, stale, "--pool", "fusion", "--shard", "1/2");
        var second = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, stale, "--pool", "fusion", "--shard", "2/2");
        var unweighed = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "fusion", "--shard", "1/2");

        Assert.That(first.Units.Distinct(), Is.EqualTo(new[] { "FusionComponents.FusionGrid" }));
        Assert.That(second.Units, Is.EqualTo(new[] { "FusionComponents.FusionMultiSelect", "FusionTemplates" }));
        Assert.That(unweighed.Units.Distinct(), Is.EqualTo(new[] { "FusionComponents.FusionGrid", "FusionComponents.FusionMultiSelect", "FusionTemplates" }));
    }

    [RefusedCase]
    public void a_shard_that_takes_no_unit_a_shard_of_no_pool_and_a_pool_that_is_none_are_refused()
    {
        using var written = new WrittenFolder();

        Assert.That(Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "hosts", "--shard", "2/2").Errors.Trim(),
            Is.EqualTo("[playwright:select] shard 2/2 of the pool 'hosts' takes no unit; the pool holds 1."));
        Assert.That(Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "fusion", "--shard", "2/2").Errors.Trim(),
            Is.EqualTo("[playwright:select] shard 2/2 of the pool 'fusion' takes no unit; the pool holds 3."));
        Assert.That(Select(written, TheSpec, TheAssembly, "--pool", "fusion").Errors.Trim(), Is.EqualTo("[playwright:select] the pool 'fusion' holds no test of the assembly."));
        Assert.That(Select(written, TheSpec, TheAssembly, "--unit", "U4", "--shard", "1/2").Errors.Trim(),
            Is.EqualTo("[playwright:select] --shard parts a pool, and 'U4' is none; a shard is taken with --pool."));
        Assert.That(Select(written, TheSpec, TheAssembly, "--pool", "core", "--shard", "3/2").Errors.Trim(),
            Is.EqualTo("[playwright:select] --shard takes <i>/<k>, the i-th of k shards, and '3/2' is not that."));
        Assert.That(Select(written, TheSpec, TheAssembly, "--pool", "native").Errors.Trim(), Is.EqualTo("[playwright:select] 'native' is no pool; the pools are core, hosts, fusion."));
    }

    [RefusedCase]
    public void a_test_the_spec_lands_and_nobody_wrote_is_refused()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly.Where(test => !test.StartsWith("M2.F1 X2.1 0", StringComparison.Ordinal)).ToArray(), "--unit", "U4");

        Assert.That(selected.Errors.Trim(), Is.EqualTo("[playwright:select] 'U4' selects 2 tests of the spec; the assembly holds no test for 1 of them, the first 'M2.F1 X2.1 0'."));
        Assert.That(selected.ExitCode, Is.EqualTo(2));
    }

    [RefusedCase]
    public void a_selector_that_takes_no_test_is_refused()
    {
        using var written = new WrittenFolder();

        Assert.That(Select(written, TheSpec, TheAssembly, "--unit", "U9").Errors.Trim(), Is.EqualTo("[playwright:select] 'U9' selects no test."));
        Assert.That(Select(written, TheSpec, TheAssembly, "--unit", "eight").Errors.Trim(), Is.EqualTo("[playwright:select] 'eight' is no unit; a unit is written U and its number."));
        Assert.That(Select(written, TheSpec, TheAssembly, "Tests").Errors.Trim(), Is.EqualTo("[playwright:select] 'Tests' selects no test."));
    }

    [RefusedCase]
    public void a_selection_no_host_served_here_runs_is_refused()
    {
        using var written = new WrittenFolder();

        var selected = Select(written, TheSpec, TheAssembly, "--unit", "U5");

        Assert.That(selected.Errors.Trim(), Is.EqualTo(
            "[playwright:select] 'U5' selects no test under a host the conformance app serves on this machine; the spec runs what it selects, 1, under other hosts."));
        Assert.That(selected.ExitCode, Is.EqualTo(2));
    }

    [RefusedCase]
    public void two_tests_on_one_promise_and_a_test_on_no_promise_of_the_spec_are_refused()
    {
        using var written = new WrittenFolder();

        var twice = Select(written, TheSpec, [.. TheAssembly, $"M1.F1 X1.1 0\t{Rounds}\tthe_round_is_closed"], "--all");
        var unknown = Select(written, TheSpec, [.. TheAssembly, $"M9.F9 X9.9 0\t{Rounds}\tthe_round_is_closed"], "--all");

        Assert.That(twice.Errors.Trim(), Is.EqualTo("[playwright:select] two tests of the assembly carry 'M1.F1 X1.1 0': the_resident_is_listed and the_round_is_closed."));
        Assert.That(unknown.Errors.Trim(), Is.EqualTo(
            "[playwright:select] the assembly holds a test on 'M9.F9 X9.9 0', no promise of the spec, 1 such in all; build the test project again."));
    }

    [RefusedCase]
    public void a_run_with_two_selectors_or_none_is_refused()
    {
        using var written = new WrittenFolder();

        Assert.That(Select(written, TheSpec, TheAssembly, "--all", "--unit", "U4").Errors, Does.StartWith("[playwright:select] a run takes one selector, and 2 were given."));
        Assert.That(Select(written, TheSpec, TheAssembly).Errors, Does.StartWith("[playwright:select] a run takes one selector, and 0 were given."));
    }

    private sealed record Selected(IReadOnlyList<string> Cases, IReadOnlyList<string> Units, string Filter, string Label, string Shard, string Errors, int ExitCode);

    private static Selected Select(WrittenFolder written, string[] spec, string[] assembly, params string[] selector)
    {
        var holdsWeights = selector.Length > 0 && selector[0].StartsWith('{');
        var weights = written.Write("shard-weights.json", holdsWeights ? selector[0] : "{}");
        var selection = written.Write("selection.json", "");
        var run = Run("node",
        [
            Path.Combine(Repository.Root, Selector), "--spec", written.Write("landings.tsv", string.Join("\n", spec) + "\n"),
            "--assembly", written.Write("tests.tsv", string.Join("\n", assembly) + "\n"), "--root", "Tests", "--weights", weights, "--out", selection,
            .. selector.Skip(holdsWeights ? 1 : 0),
        ]);
        if (run.ExitCode != 0) return new Selected([], [], "", "", "", run.Errors, run.ExitCode);

        using var read = JsonDocument.Parse(File.ReadAllText(selection));
        var filter = read.RootElement.GetProperty("filter").GetString()!;
        Assert.That(run.Output.TrimEnd('\n'), Is.EqualTo(filter), "The selector prints one filter and writes another");
        var cases = read.RootElement.GetProperty("cases").EnumerateArray().Select(one => one.GetString()!).ToList();
        return new Selected(
            cases,
            cases.Select(one => read.RootElement.GetProperty("units").GetProperty(one).GetString()!).ToList(),
            filter,
            read.RootElement.GetProperty("label").GetString()!,
            read.RootElement.GetProperty("shard").GetString()!,
            run.Errors,
            0);
    }

    private static (string Output, string Errors, int ExitCode) Run(string program, string[] arguments)
    {
        var start = new ProcessStartInfo(program, arguments)
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var started = Process.Start(start) ?? throw new InvalidOperationException(program + " could not be started");
        var output = started.StandardOutput.ReadToEndAsync();
        var errors = started.StandardError.ReadToEndAsync();
        started.WaitForExit();
        return (output.Result, errors.Result, started.ExitCode);
    }
}
