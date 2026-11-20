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
    private const string Residents = "Tests.Residents.WhenAResidentIsListed";
    private const string ResidentsFolder = "Residents/List";
    private const string Rounds = "Tests.Rounds.WhenARoundStarts";
    private const string RoundsFolder = "Rounds/Round";
    private const string Worlds = "Tests.Harness.WhenTwoWorldsRun";
    private const string WorldsFolder = "Harness";
    private const string Fresh = "Tests.RenderingAndDelivery.WhenAFreshAppOpens";
    private const string FreshFolder = "RenderingAndDelivery/Fresh";

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
        $"M1.F1 X1.1 0\t{Residents}\tthe_resident_is_listed\t{ResidentsFolder}",
        $"M1.F1 X1.2 0\t{Residents}\tthe_resident_is_opened\t{ResidentsFolder}",
        $"M2.F1 X2.1 0\t{Rounds}\tthe_round_is_listed\t{RoundsFolder}",
        $"M2.F1 X2.2 0\t{Rounds}\tthe_round_is_signed\t{RoundsFolder}",
        $"harness WhenTwoWorldsRun.each_world_lists_its_own\t{Worlds}\teach_world_lists_its_own\t{WorldsFolder}",
    ];

    private const string Grid = "Tests.FusionComponents.WhenAGridSorts";
    private const string GridFolder = "FusionComponents/FusionGrid";
    private const string MultiSelect = "Tests.FusionComponents.WhenAnItemIsPicked";
    private const string MultiSelectFolder = "FusionComponents/FusionMultiSelect";
    private const string Badge = "Tests.FusionTemplates.WhenABadgeRenders";
    private const string BadgeFolder = "FusionTemplates/Residents";

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
        $"M21.F3 M21.X1 0\t{Grid}\tthe_grid_sorts\t{GridFolder}",
        $"M21.F2 M21.X2 0\t{MultiSelect}\tthe_item_is_picked\t{MultiSelectFolder}",
        $"M22.F1 M22.X1 0\t{Badge}\tthe_badge_renders\t{BadgeFolder}",
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
        string[] withItsTest = [.. TheAssembly, $"M1.F10 X1.1 0\t{Residents}\tthe_resident_is_moved\t{ResidentsFolder}"];

        Assert.That(Select(written, withALongerFeature, withItsTest, "--feature", "M1.F1").Cases,
            Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_listed", $"{Residents}(\"net10\").the_resident_is_opened" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "--screen", "round").Cases, Is.EqualTo(new[] { $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "--test", "M1_F1.X1_1_0").Cases, Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_listed" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "Harness").Cases, Is.EqualTo(new[] { $"{Worlds}(\"net10\").each_world_lists_its_own" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "WhenARoundStarts").Cases, Is.EqualTo(new[] { $"{Rounds}(\"snakeCase\").the_round_is_listed" }));
        Assert.That(Select(written, TheSpec, TheAssembly, "List").Cases,
            Is.EqualTo(new[] { $"{Residents}(\"net10\").the_resident_is_listed", $"{Residents}(\"net10\").the_resident_is_opened" }));
    }

    [AcceptedCase]
    public void a_fixture_whose_name_opens_another_fixtures_name_is_taken_without_the_other()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpec, "M2.F1 X2.3 0\tM2_F1.X2_3_0\tU6\tround\tsnakeCase\tpool"];
        string[] assembly = [.. TheAssembly, $"M2.F1 X2.3 0\t{Rounds}Late\tthe_late_round_is_listed\t{RoundsFolder}"];

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
    public void a_fresh_hosts_selection_names_the_fresh_app_the_run_builds_and_a_selection_served_here_names_none()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpec, "M19.F1 M19.X5 0\tM19_F1.M19_X5_0\tU22\tfresh\tconsumerNet10\tfresh"];
        string[] assembly = [.. TheAssembly, $"M19.F1 M19.X5 0\t{Fresh}\tthe_plan_boots\t{FreshFolder}"];

        var fresh = Select(written, spec, assembly, "--test", "M19_F1.M19_X5_0", "--host", "consumerNet10");
        var servedHere = Select(written, spec, assembly, "--all");

        Assert.That(fresh.Cases, Is.EqualTo(new[] { $"{Fresh}(\"consumerNet10\").the_plan_boots" }));
        Assert.That(fresh.Units, Is.EqualTo(new[] { "fresh" }));
        Assert.That(fresh.Filter, Is.EqualTo($"FullyQualifiedName~{Fresh}&TestCategory=consumerNet10"));
        Assert.That(fresh.Label, Is.EqualTo("M19_F1.M19_X5_0@consumerNet10"));
        Assert.That(fresh.FreshApps, Is.EqualTo(new[] { "tests/Alis.Reactive.Consumer.Net10" }));
        Assert.That(servedHere.Cases, Does.Not.Contain($"{Fresh}(\"consumerNet10\").the_plan_boots"));
        Assert.That(servedHere.FreshApps, Is.Empty);
    }

    [AcceptedCase]
    public void only_a_net10_move_in_placement_taken_on_this_shard_prepares_the_move_in_page()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpec, "M9.F2 M9.X47 0\tM9_F2.M9_X47_0\tU23\tmoveIn\tnet10\tpool", "M9.F2 M9.X47 0\tM9_F2.M9_X47_0\tU23\tmoveIn\tnet48\ttwin"];
        string[] assembly = [.. TheAssembly, "M9.F2 M9.X47 0\tTests.PartialsAndSlots.WhenAContactsRoleNeedsItsAuthority\ta_revoked_authority_reference_is_refused_by_the_server_with_its_reason\tPartialsAndSlots/MoveIn"];
        const string weights = "{ \"PartialsAndSlots\": 5, \"Residents\": 3, \"Harness\": 1 }";

        Assert.That(Select(written, spec, assembly, "--test", "M9_F2.M9_X47_0").PrepareMoveIn, Is.EqualTo(new[] { "net10" }));
        Assert.That(Select(written, spec, assembly, weights, "--pool", "core", "--shard", "1/2").PrepareMoveIn, Is.EqualTo(new[] { "net10" }));
        Assert.That(Select(written, spec, assembly, weights, "--pool", "core", "--shard", "2/2").PrepareMoveIn, Is.Empty);
        Assert.That(Select(written, spec, assembly, "--all", "--host", "net48").PrepareMoveIn, Is.Empty);
        Assert.That(Select(written, spec, assembly, "--test", "M1_F1.X1_1_0").PrepareMoveIn, Is.Empty);
    }

    [AcceptedCase]
    public void the_selected_admissions_move_in_requests_readiness_while_other_variants_skip_it()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpec,
            "M9.F2 M9.X270 0\tM9_F2.M9_X270_0\tU23\tmoveIn\tadmissions\tpool",
            "M9.F2 M9.X262 0\tM9_F2.M9_X262_0\tU23\tmoveIn\tpublicInformation\tpool",
            "M9.F2 M9.X261 0\tM9_F2.M9_X261_0\tU23\tmoveIn\tantiforgeryField\tpool",
            "M9.F2 M9.X47 0\tM9_F2.M9_X47_0\tU23\tmoveIn\tnet10\tpool"];
        string[] assembly = [.. TheAssembly,
            "M9.F2 M9.X270 0\tTests.PartialsAndSlots.WhenAdmissionSaves\ta_refused_flush_leaves_handover_pending\tPartialsAndSlots/MoveIn",
            "M9.F2 M9.X262 0\tTests.PartialsAndSlots.WhenThePublicReads\tthe_desk_is_available\tPartialsAndSlots/MoveIn",
            "M9.F2 M9.X261 0\tTests.PartialsAndSlots.WhenReceptionSignsIn\tthe_visitor_is_registered\tPartialsAndSlots/MoveIn",
            "M9.F2 M9.X47 0\tTests.PartialsAndSlots.WhenAContactNeedsAuthority\tthe_authority_is_available\tPartialsAndSlots/MoveIn"];

        Assert.That(Select(written, spec, assembly, "--test", "M9_F2.M9_X270_0").PrepareMoveIn, Is.EqualTo(new[] { "admissions" }));
        Assert.That(Select(written, spec, assembly, "--screen", "moveIn").PrepareMoveIn, Is.EqualTo(new[] { "admissions", "net10" }));
        Assert.That(Select(written, spec, assembly, "--host", "publicInformation", "--screen", "moveIn").PrepareMoveIn, Is.Empty);
        Assert.That(Select(written, spec, assembly, "--host", "antiforgeryField", "--screen", "moveIn").PrepareMoveIn, Is.Empty);
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
    public void a_selection_names_the_tests_of_its_fusion_fixtures()
    {
        using var written = new WrittenFolder();

        var all = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--all");
        var core = Select(written, TheSpecWithFusion, TheAssemblyWithFusion, "--pool", "core");

        Assert.That(all.Fusion, Is.EqualTo(new[]
        {
            $"{Grid}(\"fusionShell\").the_grid_sorts",
            $"{Grid}(\"net10\").the_grid_sorts",
            $"{MultiSelect}(\"net10\").the_item_is_picked",
            $"{Badge}(\"net10\").the_badge_renders",
        }));
        Assert.That(all.Cases, Has.Count.EqualTo(9));
        Assert.That(core.Fusion, Is.Empty);
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
    public void two_fixtures_in_one_slices_folder_are_one_shard_unit()
    {
        using var written = new WrittenFolder();
        string[] spec = [.. TheSpecWithFusion, "M21.F2 M21.X2 1\tM21_F2.M21_X2_1\tU25\tbench\tnet10\tpool"];
        string[] assembly = [.. TheAssemblyWithFusion, $"M21.F2 M21.X2 1\tTests.FusionComponents.WhenAnItemIsCleared\tthe_item_is_cleared\t{MultiSelectFolder}"];

        var fusion = Select(written, spec, assembly, "--pool", "fusion");

        Assert.That(fusion.Units.Distinct(), Is.EqualTo(new[] { "FusionComponents.FusionGrid", "FusionComponents.FusionMultiSelect", "FusionTemplates" }));
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

    [AcceptedCase]
    public void every_host_takes_the_full_list_the_twins_tests_among_it()
    {
        using var written = new WrittenFolder();

        var full = Select(written, TheSpec, TheAssembly, "--all", "--every-host");

        Assert.That(full.Cases, Is.EqualTo(new[]
        {
            $"{Worlds}(\"net10\").each_world_lists_its_own",
            $"{Residents}(\"net10\").the_resident_is_listed",
            $"{Residents}(\"net10\").the_resident_is_opened",
            $"{Residents}(\"net48\").the_resident_is_opened",
            $"{Rounds}(\"net48\").the_round_is_signed",
            $"{Rounds}(\"snakeCase\").the_round_is_listed",
        }));
        Assert.That(full.Filter, Is.Empty);
        Assert.That(full.Label, Is.EqualTo("all@every host"));
    }

    [RefusedCase]
    public void every_host_beside_one_host_is_refused()
    {
        using var written = new WrittenFolder();

        Assert.That(Select(written, TheSpec, TheAssembly, "--all", "--every-host", "--host", "net48").Errors.Trim(),
            Is.EqualTo("[playwright:select] --every-host and --host 'net48' name two sets of hosts; a selection takes one."));
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

        var twice = Select(written, TheSpec, [.. TheAssembly, $"M1.F1 X1.1 0\t{Rounds}\tthe_round_is_closed\t{RoundsFolder}"], "--all");
        var unknown = Select(written, TheSpec, [.. TheAssembly, $"M9.F9 X9.9 0\t{Rounds}\tthe_round_is_closed\t{RoundsFolder}"], "--all");

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

    private sealed record Selected(IReadOnlyList<string> Cases, IReadOnlyList<string> Units, string Filter, string Label, string Shard, IReadOnlyList<string> FreshApps, IReadOnlyList<string> Fusion, IReadOnlyList<string> PrepareMoveIn, string Errors, int ExitCode);

    private static Selected Select(WrittenFolder written, string[] spec, string[] assembly, params string[] selector)
    {
        var holdsWeights = selector.Length > 0 && selector[0].StartsWith('{');
        var weights = written.Write("shard-weights.json", holdsWeights ? selector[0] : "{}");
        var selection = written.Write("selection.json", "");
        var run = Run("node",
        [
            Path.Combine(Repository.Root, Selector), "--spec", written.Write("landings.tsv", string.Join("\n", spec) + "\n"),
            "--assembly", written.Write("tests.tsv", string.Join("\n", assembly) + "\n"), "--weights", weights, "--out", selection,
            .. selector.Skip(holdsWeights ? 1 : 0),
        ]);
        if (run.ExitCode != 0) return new Selected([], [], "", "", "", [], [], [], run.Errors, run.ExitCode);

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
            read.RootElement.GetProperty("freshApps").EnumerateArray().Select(one => one.GetString()!).ToList(),
            read.RootElement.GetProperty("fusion").EnumerateArray().Select(one => one.GetString()!).ToList(),
            read.RootElement.GetProperty("prepareMoveIn").EnumerateArray().Select(one => one.GetString()!).ToList(),
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
