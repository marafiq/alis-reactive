using System.Text.Json;
using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationMirrorsItsVendor
{
    private const string Board = """
        type	ward	Board	class		ward:Panel
        type	ward	Panel	class		
        type	ward	pager	function	callable	
        type	ward	paging	function	callable,promise	
        member	ward	Panel	heading	property	
        member	ward	Board	title	property	
        member	ward	Board	status	readonly	
        member	ward	Board	pin	writeonly	
        member	ward	Board	beds	property	readsnull,writesnull
        member	ward	Board	census	property	readsnull
        member	ward	Board	lead	property	readsnull
        member	ward	Board	label	property	writesnull
        member	ward	Board	refresh	method	callable
        member	ward	Board	reload	method	callable,promise
        member	ward	Board	sync	method	callable,mixed
        member	ward	Board	nurse	method	callable,returnsnull
        member	ward	Board	onring	property	readsnull,writesnull,callable
        """;

    private static string Declaring(string members, string mirrors = """[Mirrors("ward", "Board")]""") => $$"""
        using System.Diagnostics.CodeAnalysis;
        using Alis.Reactive.Components;

        namespace Ward;

        {{mirrors}}
        public sealed class WardBoard : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{members}}
        }
        """;

    [Test]
    public Task a_component_that_names_no_type_is_reported_with_the_door_that_names_one()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS014",
            "'WardBoard' names no TypeScript type it mirrors: mark it [Mirrors(module, type)] with the type its vendor's declarations give it",
            """
            using Alis.Reactive.Components;

            namespace Ward;

            public sealed class [|WardBoard|] : Component
            {
                public override ComponentVendor Vendor => ComponentVendor.Native;
            }
            """,
            Board);

    [Test]
    public Task a_plugin_that_names_no_type_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS014",
            "'Pager' names no TypeScript type it mirrors: mark it [Mirrors(module, type)] with the type its vendor's declarations give it",
            """
            using Alis.Reactive.Components;

            namespace Ward;

            public sealed class [|Pager|] : Plugin
            {
                public Pager()
                    : base("pager")
                {
                }
            }
            """,
            Board);

    [Test]
    public Task an_abstract_declaration_names_no_type()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            """
            using Alis.Reactive.Components;

            namespace Ward;

            public abstract class WardComponent : Component
            {
            }
            """,
            Board);

    [Test]
    public Task a_type_the_contract_does_not_list_is_reported_at_the_attribute()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS015",
            "'WardBoard' mirrors 'Bord' of 'ward', which the vendor contract does not list",
            Declaring("", """[[|Mirrors("ward", "Bord")|]]"""),
            Board);

    [Test]
    public Task a_type_its_module_declares_twice_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS015",
            "'WardBoard' mirrors 'Board' of 'ward', which its module declares more than once",
            Declaring("", """[[|Mirrors("ward", "Board")|]]"""),
            "type\tward\tBoard\tduplicate\t\t\n");

    [Test]
    public Task a_build_with_no_contract_is_reported_with_the_targets_to_import()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS015",
            "'WardBoard' mirrors 'Board' of 'ward', and no vendor contract reached the build: import poc/VendorContract.targets",
            Declaring("", """[[|Mirrors("ward", "Board")|]]"""),
            null);

    [Test]
    public Task members_the_type_and_its_bases_make_public_are_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                public Property<string> Title => Property<string>();

                public ReadOnly<string> Status => ReadOnly<string>();

                public WriteOnly<string> Pin => WriteOnly<string>();

                public Property<string> Heading => Property<string>();

                public ReadOnly<string> Caption => ReadOnly<string>(name: "title");

                public void Refresh() => Method();

                public void Reload() => MethodAsync();
            """),
            Board);

    [Test]
    public Task a_member_the_type_does_not_make_public_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'WardBoard.Wing' declares 'wing', which 'Board' does not make public: declare only what the vendor's typed API makes public",
            Declaring("""
                public ReadOnly<string> [|Wing|] => ReadOnly<string>();
            """),
            Board);

    [Test]
    public Task a_member_named_its_own_way_is_found_by_that_name()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'WardBoard.Title' declares 'heading_text', which 'Board' does not make public: declare only what the vendor's typed API makes public",
            Declaring("""
                public Property<string> [|Title|] => Property<string>(name: "heading_text");
            """),
            Board);

    [Test]
    public Task a_method_declared_with_method_whose_type_returns_a_promise_is_reported_with_method_async()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS017",
            "'WardBoard.Reload' is declared with Method, and 'Board.reload' returns a promise: declare it with MethodAsync",
            Declaring("""
                public void [|Reload|]() => Method();
            """),
            Board);

    [Test]
    public Task a_method_declared_with_method_async_whose_type_returns_no_promise_is_reported_with_method()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS017",
            "'WardBoard.Refresh' is declared with MethodAsync, and 'Board.refresh' returns no promise: declare it with Method",
            Declaring("""
                public void [|Refresh|]() => MethodAsync();
            """),
            Board);

    [Test]
    public Task a_method_whose_signatures_disagree_on_a_promise_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS017",
            "'WardBoard.Sync' is declared with Method, and 'Board.sync' returns a promise from some of its signatures and not from others, which no declaration mirrors",
            Declaring("""
                public void [|Sync|]() => Method();
            """),
            Board);

    [Test]
    public Task a_property_declared_on_a_read_only_member_is_reported_with_read_only()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'WardBoard.Status' is declared with Property, and 'Board.status' is a read-only value, not a method: declare it with ReadOnly",
            Declaring("""
                public Property<string> [|Status|] => Property<string>();
            """),
            Board);

    [Test]
    public Task a_read_of_a_write_only_member_is_reported_with_write_only()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'WardBoard.Pin' is declared with ReadOnly, and 'Board.pin' is write-only: declare it with WriteOnly",
            Declaring("""
                public ReadOnly<string> [|Pin|] => ReadOnly<string>();
            """),
            Board);

    [Test]
    public Task a_method_door_on_a_value_is_reported_with_its_value_door()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'WardBoard.Status' is declared with Method, and 'Board.status' is a read-only value, not a method: declare it with ReadOnly",
            Declaring("""
                public void [|Status|]() => Method();
            """),
            Board);

    [Test]
    public Task a_value_door_on_a_method_is_reported_with_the_method_doors()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'WardBoard.Refresh' is declared with ReadOnly, and 'Board.refresh' is a method: declare it with Method or MethodAsync",
            Declaring("""
                public ReadOnly<string> [|Refresh|] => ReadOnly<string>();
            """),
            Board);

    [Test]
    public Task a_method_door_on_a_property_that_holds_a_function_calls_it()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                public void Ring() => Method(name: "onring");
            """),
            Board);

    [Test]
    public Task a_read_only_door_on_a_read_write_member_reads_it()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                public ReadOnly<string> Title => ReadOnly<string>();
            """),
            Board);

    [Test]
    public Task a_read_of_a_member_that_can_read_null_into_a_type_that_cannot_hold_it_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardBoard.Census' reads 'int', and 'Board.census' can read null or undefined: declare 'int?'",
            Declaring("""
                public Property<int> [|Census|] => Property<int>();
            """),
            Board);

    [Test]
    public Task a_reference_read_that_can_read_null_into_a_type_that_cannot_hold_it_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardBoard.Lead' reads 'string', and 'Board.lead' can read null or undefined: declare 'string?'",
            Declaring("""
                public ReadOnly<string> [|Lead|] => ReadOnly<string>();
            """),
            Board);

    [Test]
    public Task a_write_of_null_into_a_member_that_takes_none_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardBoard.Title' writes 'string?', and 'Board.title' takes no null: declare 'string'",
            Declaring("""
                public Property<string?> [|Title|] => Property<string?>();
            """),
            Board);

    [Test]
    public Task a_nullable_declaration_of_a_member_that_reads_none_and_takes_null_fits_both_ways()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                public Property<string?> Label => Property<string?>();

                public Property<int?> Beds => Property<int?>();

                public ReadOnly<string?> Lead => ReadOnly<string?>();
            """),
            Board);

    [Test]
    public Task a_result_that_can_be_null_into_a_type_that_cannot_hold_it_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardBoard.Nurse' returns 'string', and 'Board.nurse' can return null or undefined: declare 'string?'",
            Declaring("""
                public MethodResult<string> [|Nurse|]() => Method<string>();
            """),
            Board);

    [Test]
    public Task a_result_declared_nullable_holds_null()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                public MethodResult<string?> Nurse() => Method<string?>();
            """),
            Board);

    [Test]
    public Task a_plugin_that_calls_itself_mirrors_a_function()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            """
            using Alis.Reactive.Components;
            using Alis.Reactive.Values;

            namespace Ward;

            [Mirrors("ward", "pager")]
            public sealed class Pager : Plugin
            {
                public Pager()
                    : base("pager")
                {
                }

                public MethodResult<string> Page(Arg<string> nurse) => CallItself<string>([nurse]);
            }
            """,
            Board);

    [Test]
    public Task a_plugin_that_calls_a_type_that_is_not_callable_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'Pager.Page' is declared with CallItself, and 'Board' is not callable: declare it with the members it has",
            """
            using Alis.Reactive.Components;

            namespace Ward;

            [Mirrors("ward", "Board")]
            public sealed class Pager : Plugin
            {
                public Pager()
                    : base("pager")
                {
                }

                public void [|Page|]() => CallItself();
            }
            """,
            Board);

    [Test]
    public Task a_plugin_that_calls_itself_where_the_function_returns_a_promise_is_reported_as_a_gap()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS017",
            "'Paging.Page' is declared with CallItself, and 'paging' returns a promise: declare it with a door that awaits a call of the object itself, a gap to name",
            """
            using Alis.Reactive.Components;

            namespace Ward;

            [Mirrors("ward", "paging")]
            public sealed class Paging : Plugin
            {
                public Paging()
                    : base("paging")
                {
                }

                public void [|Page|]() => CallItself();
            }
            """,
            Board);

    [Test]
    public Task a_drift_a_test_arranges_on_purpose_is_suppressed_by_its_justification()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""
                [SuppressMessage("Alis.Reactive.Contract", "ALIS017", Justification = "Drift on purpose: a test proves the browser stops on it.")]
                public void Reload() => Method();
            """),
            Board);

    [TestCase("URLValue")]
    [TestCase("IOStream")]
    [TestCase("ID")]
    [TestCase("Id")]
    [TestCase("A")]
    [TestCase("ABC")]
    [TestCase("XMLHttpRequest")]
    [TestCase("LastNote")]
    [TestCase("TickLabel")]
    [TestCase("OnRing")]
    [TestCase("lowercase")]
    [TestCase("Bed2Door")]
    [TestCase("ÉtatLit")]
    public Task a_members_name_is_found_as_system_text_json_camel_cases_it(string member)
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring($$"""
                public ReadOnly<string> {{member}} => ReadOnly<string>();
            """),
            $"type\tward\tBoard\tclass\t\t\nmember\tward\tBoard\t{JsonNamingPolicy.CamelCase.ConvertName(member)}\treadonly\t\n");
}
