using Alis.Reactive.Analyzers.SliceTree;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

[TestFixture]
public class SliceTreeTests
{
    private const string DesignSystem = "Alis.Reactive.DesignSystem";
    private const string DesignSystemTable = "Tokens\nLayout\n";
    private const string Fusion = "Alis.Reactive.Fusion";
    private const string FusionTable = ".\nTemplates\nAppLevel/* Components/*\n";
    private const string TagHelpers = "Alis.Reactive.NativeTagHelpers";
    private const string TagHelpersTable = ".\n*\n";

    private static string Upward(string from, string symbol, string to) =>
        $"'{from}' uses '{symbol}' of slice '{to}', a row above it in SliceTree.txt; a slice uses only slices in rows below it";

    private static string SameRow(string from, string symbol, string to) =>
        $"'{from}' uses '{symbol}' of slice '{to}', the same row in SliceTree.txt; a slice uses only slices in rows below it";

    private static string WrongNamespace(string file, string actual, string expected) =>
        $"'{file}' is in namespace '{actual}'; expected '{expected}': a file that declares a public type has the package namespace, any other file's namespace is its folder";

    private static string Disagree(string what) => $"SliceTree.txt and the package disagree: {what}";

    private static readonly (string Path, string Source) Stack = ("Layout/Stack.cs",
        """
        namespace Alis.Reactive.DesignSystem.Layout;

        internal class Stack
        {
            internal static int Width() => 1;
        }
        """);

    [Test]
    public Task a_call_into_a_slice_above_is_reported_where_it_is_written() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS010",
            Upward("Tokens", "Alis.Reactive.DesignSystem.Layout.Stack.Width()", "Layout"),
            ("Tokens/Color.cs",
                """
                using Alis.Reactive.DesignSystem.Layout;

                namespace Alis.Reactive.DesignSystem.Tokens;

                internal static class Color
                {
                    internal static int Width() => [|Stack.Width()|];
                }
                """),
            Stack);

    [Test]
    public Task a_base_type_in_a_slice_above_is_reported_at_the_class() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS010",
            Upward("Tokens", "Alis.Reactive.DesignSystem.Layout.Stack", "Layout"),
            ("Tokens/Color.cs",
                """
                using Alis.Reactive.DesignSystem.Layout;

                namespace Alis.Reactive.DesignSystem.Tokens;

                internal sealed class [|Color|] : Stack
                {
                }
                """),
            Stack);

    [Test]
    public Task a_generic_argument_in_a_slice_above_is_reported_at_the_parameter() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS010",
            Upward("Tokens", "Alis.Reactive.DesignSystem.Layout.Stack", "Layout"),
            ("Tokens/Color.cs",
                """
                using System.Collections.Generic;
                using Alis.Reactive.DesignSystem.Layout;

                namespace Alis.Reactive.DesignSystem.Tokens;

                internal static class Color
                {
                    internal static void Take(List<Stack> [|items|])
                    {
                    }
                }
                """),
            Stack);

    [Test]
    public Task two_slices_of_one_wildcard_row_using_each_other_are_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(Fusion, FusionTable, "ALIS010",
            SameRow("Components/FusionGrid", "Alis.Reactive.Fusion.Components.FusionToast.ToastHelper.Text", "Components/FusionToast"),
            ("FusionThing.cs",
                """
                namespace Alis.Reactive.Fusion;

                internal static class FusionThing
                {
                }
                """),
            ("Templates/Template.cs",
                """
                namespace Alis.Reactive.Fusion.Templates;

                internal static class Template
                {
                }
                """),
            ("AppLevel/FusionDrawer/DrawerHelper.cs",
                """
                namespace Alis.Reactive.Fusion.AppLevel.FusionDrawer;

                internal static class DrawerHelper
                {
                }
                """),
            ("Components/FusionGrid/GridHelper.cs",
                """
                using Alis.Reactive.Fusion.Components.FusionToast;

                namespace Alis.Reactive.Fusion.Components.FusionGrid;

                internal static class GridHelper
                {
                    internal static string Note() => [|ToastHelper.Text|];
                }
                """),
            ("Components/FusionToast/ToastHelper.cs",
                """
                namespace Alis.Reactive.Fusion.Components.FusionToast;

                internal static class ToastHelper
                {
                    internal static string Text => "";
                }
                """));

    [Test]
    public Task each_top_folder_of_a_star_row_is_its_own_slice() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(TagHelpers, TagHelpersTable, "ALIS010",
            SameRow("Card", "Alis.Reactive.NativeTagHelpers.Kv.KvTagHelper.Open()", "Kv"),
            ("Spacing.cs",
                """
                namespace Alis.Reactive.NativeTagHelpers;

                internal static class Spacing
                {
                }
                """),
            ("Card/CardTagHelper.cs",
                """
                using Alis.Reactive.NativeTagHelpers.Kv;

                namespace Alis.Reactive.NativeTagHelpers.Card;

                internal static class CardTagHelper
                {
                    internal static string Open() => [|KvTagHelper.Open()|];
                }
                """),
            ("Kv/KvTagHelper.cs",
                """
                namespace Alis.Reactive.NativeTagHelpers.Kv;

                internal static class KvTagHelper
                {
                    internal static string Open() => "";
                }
                """));

    [Test]
    public Task the_root_files_are_the_lowest_slice() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(Fusion, FusionTable, "ALIS010",
            Upward(".", "Alis.Reactive.Fusion.Templates.Template.Name", "Templates"),
            ("FusionThing.cs",
                """
                using Alis.Reactive.Fusion.Templates;

                namespace Alis.Reactive.Fusion;

                internal static class FusionThing
                {
                    internal static string Name() => [|Template.Name|];
                }
                """),
            ("Templates/Template.cs",
                """
                namespace Alis.Reactive.Fusion.Templates;

                internal static class Template
                {
                    internal static string Name => "";
                }
                """),
            ("AppLevel/FusionDrawer/DrawerHelper.cs",
                """
                namespace Alis.Reactive.Fusion.AppLevel.FusionDrawer;

                internal static class DrawerHelper
                {
                }
                """),
            ("Components/FusionGrid/GridHelper.cs",
                """
                namespace Alis.Reactive.Fusion.Components.FusionGrid;

                internal static class GridHelper
                {
                }
                """));

    [Test]
    public Task a_slice_using_a_slice_below_it_is_not_reported() =>
        Fixture.ReportsNothingInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable,
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem.Tokens;

                internal static class Color
                {
                    internal static int Value => 1;
                }
                """),
            ("Layout/Stack.cs",
                """
                using Alis.Reactive.DesignSystem.Tokens;

                namespace Alis.Reactive.DesignSystem.Layout;

                internal static class Stack
                {
                    internal static int Width() => Color.Value;
                }
                """));

    [Test]
    public Task an_applications_own_folders_are_left_alone() =>
        Fixture.ReportsNothingInPackage<SliceTreeAnalyzer>(Fixture.AppAssembly, table: null,
            ("Tokens/Color.cs",
                """
                using MapleGrove.Web.Layout;

                namespace MapleGrove.Web;

                public class Color
                {
                    public static int Width() => Stack.Width();
                }
                """),
            ("Layout/Stack.cs",
                """
                namespace MapleGrove.Web.Layout;

                public class Stack
                {
                    public static int Width() => 1;
                }
                """));

    [Test]
    public Task a_folder_without_a_row_is_reported_at_its_file() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS013",
            Disagree("folder 'Themes' has no row"),
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem.Tokens;

                internal static class Color
                {
                }
                """),
            Stack,
            ("Themes/Dark.cs",
                """
                [|namespace|] Alis.Reactive.DesignSystem.Themes;

                internal static class Dark
                {
                }
                """));

    [Test]
    public Task a_row_entry_that_names_no_folder_is_reported_in_the_table() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, "Tokens\n[|Layout|]\n", "ALIS013",
            Disagree("entry 'Layout' names no folder"),
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem.Tokens;

                internal static class Color
                {
                }
                """));

    [Test]
    public Task a_type_declared_in_two_slices_is_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS013",
            Disagree("'Alis.Reactive.DesignSystem.Color' is declared in slices 'Tokens' and 'Layout'"),
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem;

                public sealed partial class [|Color|]
                {
                }
                """),
            ("Layout/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem;

                public sealed partial class Color
                {
                }
                """));

    [Test]
    public Task an_internal_file_in_the_package_namespace_is_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS011",
            WrongNamespace("Tokens/Color.cs", "Alis.Reactive.DesignSystem", "Alis.Reactive.DesignSystem.Tokens"),
            ("Tokens/Color.cs",
                """
                namespace [|Alis.Reactive.DesignSystem|];

                internal static class Color
                {
                }
                """),
            Stack);

    [Test]
    public Task a_public_types_file_in_its_folder_namespace_is_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS011",
            WrongNamespace("Tokens/Color.cs", "Alis.Reactive.DesignSystem.Tokens", "Alis.Reactive.DesignSystem"),
            ("Tokens/Color.cs",
                """
                namespace [|Alis.Reactive.DesignSystem.Tokens|];

                public sealed class Color
                {
                }
                """),
            Stack);

    [Test]
    public Task the_right_namespaces_are_not_reported() =>
        Fixture.ReportsNothingInPackage<SliceTreeAnalyzer>(Fusion, FusionTable,
            ("FusionThing.cs",
                """
                namespace Alis.Reactive.Fusion;

                internal static class FusionThing
                {
                }
                """),
            ("Templates/Template.cs",
                """
                namespace Alis.Reactive.Fusion.Templates;

                internal static class Template
                {
                }
                """),
            ("AppLevel/FusionDrawer/DrawerHelper.cs",
                """
                namespace Alis.Reactive.Fusion.AppLevel.FusionDrawer;

                internal static class DrawerHelper
                {
                }
                """),
            ("Components/FusionGrid/GridHelper.cs",
                """
                namespace Alis.Reactive.Fusion;

                public sealed class GridHelper
                {
                }
                """));

    [Test]
    public Task a_public_class_nothing_derives_from_is_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable, "ALIS012",
            "'Alis.Reactive.DesignSystem.Color' is public and nothing in the package derives from it; seal it",
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem;

                public class [|Color|]
                {
                }
                """),
            Stack);

    [Test]
    public Task a_public_class_with_a_subclass_in_the_package_is_not_reported() =>
        Fixture.ReportsNothingInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable,
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem;

                public class Color
                {
                }

                public sealed class Red : Color
                {
                }
                """),
            Stack);

    [Test]
    public Task abstract_static_and_internal_classes_are_not_reported() =>
        Fixture.ReportsNothingInPackage<SliceTreeAnalyzer>(DesignSystem, DesignSystemTable,
            ("Tokens/Color.cs",
                """
                namespace Alis.Reactive.DesignSystem;

                public abstract class Shade
                {
                }

                public static class Palette
                {
                }

                internal class Tint
                {
                }
                """),
            Stack);

    [Test]
    public Task a_shipped_package_without_a_table_is_reported() =>
        Fixture.ReportsInPackage<SliceTreeAnalyzer>(Fusion, table: null, "ALIS013",
            Disagree("'Alis.Reactive.Fusion' ships without a SliceTree.txt"),
            ("Templates/Form.cs",
                """
                namespace Alis.Reactive.Fusion.Templates;

                internal static class Form
                {
                }
                """));
}
