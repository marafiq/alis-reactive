using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationTypesItsValue
{
    private const string Desk = """
        type	desk	Desk	class			
        type	desk	Match	interface			
        type	desk	Wing	interface			
        type	desk	Changed	interface			
        type	desk	EventTarget	interface			
        type	desk	Box	class		desk:EventTarget	
        member	desk	Desk	term	readonly		
        member	desk	Desk	opened	readonly		
        member	desk	Desk	settled	readonly		
        member	desk	Desk	tags	readonly		
        member	desk	Desk	extra	readonly		
        member	desk	Desk	label	property		
        member	desk	Desk	note	property		
        member	desk	Desk	count	method	callable,promise	
        member	desk	Desk	match	method	callable,promise,returnsnull	
        member	desk	Desk	addEventListener	method	callable	name:changed
        member	desk	Match	name	readonly		
        member	desk	Match	wing	readonly	readsnull	
        member	desk	Wing	name	readonly		
        member	desk	Wing	floor	readonly		
        member	desk	Changed	target	readonly	readsnull	
        member	desk	Changed	source	readonly		
        member	desk	Changed	pending	readonly		
        member	desk	Box	value	property		
        holds	desk	Desk	term	value	other		string
        holds	desk	Desk	opened	value	other		string
        holds	desk	Desk	settled	value	object	lib.es5	Promise#1
        holds	desk	Desk	tags	value	other		readonly string[]
        holds	desk	Desk	extra	value	other		unknown
        holds	desk	Desk	label	value	other		string
        holds	desk	Desk	label	value	other		number
        holds	desk	Desk	note	value	other		string
        holds	desk	Desk	count	value	other		number
        holds	desk	Desk	match	value	null		
        holds	desk	Desk	match	value	object	desk	Match
        holds	desk	Desk	addEventListener	event:changed:parameter:0	object	desk	Changed
        holds	desk	Match	name	value	other		string
        holds	desk	Match	wing	value	null		
        holds	desk	Match	wing	value	object	desk	Wing
        holds	desk	Wing	name	value	other		string
        holds	desk	Wing	floor	value	other		number
        holds	desk	Changed	target	value	null		
        holds	desk	Changed	target	value	object	desk	EventTarget
        holds	desk	Changed	source	value	object	desk	Box
        holds	desk	Changed	pending	value	object	lib.es5	Promise#1
        holds	desk	Box	value	value	other		string
        """;

    private static string Declaring(string members, string types = "") => $$"""
        using System;
        using System.Collections.Generic;
        using Alis.Reactive.Components;

        namespace Ward;

        [Mirrors("desk", "Desk")]
        public sealed class FrontDesk : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{members}}
        }

        [Mirrors("desk", "Box")]
        public sealed class TextBox : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

            public Property<string> Value => Property<string>();
        }

        public enum Shift
        {
            Day,
            Night,
        }

        {{types}}
        """;

    [Test]
    public Task a_value_declared_as_a_number_on_a_member_that_holds_text_is_reported_with_string()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Term' is declared with ReadOnly<int>, and 'Desk.term' holds text: declare it as string",
            Declaring("""    public ReadOnly<int> [|Term|] => ReadOnly<int>();"""),
            Desk);

    [Test]
    public Task a_value_declared_as_the_type_its_member_holds_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""    public ReadOnly<string> Term => ReadOnly<string>();"""),
            Desk);

    [Test]
    public Task an_async_method_declared_as_text_whose_promise_resolves_to_a_number_is_reported_with_a_number_type()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Count' is declared with MethodAsync<string>, and 'Desk.count' holds a number: declare it as a number type: int, long, decimal or double",
            Declaring("""    public MethodResult<string> [|Count|]() => MethodAsync<string>();"""),
            Desk);

    [Test]
    public Task a_member_that_holds_a_promise_is_reported_as_a_gap_whatever_its_declared_type()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Settled' is declared with ReadOnly<string>, and 'Desk.settled' holds a promise: no door awaits a promise a member holds, a gap to name",
            Declaring("""    public ReadOnly<string> [|Settled|] => ReadOnly<string>();"""),
            Desk);

    [Test]
    public Task a_member_of_the_data_a_promise_resolves_to_that_its_type_does_not_hold_is_reported_on_the_declaration_with_its_path()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Match' is declared with MethodAsync<Found>, and 'Found.Wing.Nmae' reads 'nmae', which 'Wing' does not hold: name it as 'Wing' does",
            Declaring(
                """    public MethodResult<Found?> [|Match|]() => MethodAsync<Found?>();""",
                """
                public sealed record Found(string Name, Section? Wing);

                public sealed record Section(string Nmae, int Floor);
                """),
            Desk);

    [Test]
    public Task data_whose_members_its_type_holds_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring(
                """    public MethodResult<Found?> Match() => MethodAsync<Found?>();""",
                """
                public sealed record Found(string Name, Section? Wing);

                public sealed record Section(string Name, int Floor);
                """),
            Desk);

    [Test]
    public Task a_member_of_the_data_declared_as_text_where_its_type_holds_a_number_is_reported_with_its_path()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Match' is declared with MethodAsync<Found>, and 'Wing.floor' holds a number, and 'Found.Wing.Floor' is 'string': declare it as a number type: int, long, decimal or double",
            Declaring(
                """    public MethodResult<Found?> [|Match|]() => MethodAsync<Found?>();""",
                """
                public sealed record Found(string Name, Section? Wing);

                public sealed record Section(string Name, string Floor);
                """),
            Desk);

    [Test]
    public Task a_date_read_from_a_member_that_holds_text_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""    public ReadOnly<DateTime> Opened => ReadOnly<DateTime>();"""),
            Desk);

    [Test]
    public Task a_date_written_to_a_member_that_holds_text_is_reported_with_string()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Note' is declared with Property<DateTime>, and 'Desk.note' holds text: declare it as string",
            Declaring("""    public Property<DateTime> [|Note|] => Property<DateTime>();"""),
            Desk);

    [Test]
    public Task an_enum_read_from_a_member_that_holds_text_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""    public ReadOnly<Shift> Term => ReadOnly<Shift>(name: "term");"""),
            Desk);

    [Test]
    public Task a_list_of_numbers_on_a_member_that_holds_a_list_of_text_is_reported_with_a_list_of_string()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Tags' is declared with ReadOnly<IReadOnlyList<int>>, and 'Desk.tags' holds a list of text: declare it as a list of string",
            Declaring("""    public ReadOnly<IReadOnlyList<int>> [|Tags|] => ReadOnly<IReadOnlyList<int>>();"""),
            Desk);

    [Test]
    public Task a_list_of_text_on_a_member_that_holds_a_list_of_text_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""    public ReadOnly<IReadOnlyList<string>> Tags => ReadOnly<IReadOnlyList<string>>();"""),
            Desk);

    [Test]
    public Task a_member_its_vendor_leaves_untyped_takes_any_declared_type()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring("""    public ReadOnly<int> Extra => ReadOnly<int>();"""),
            Desk);

    [Test]
    public Task a_member_that_holds_text_or_a_number_is_reported_as_a_gap()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'FrontDesk.Label' is declared with Property<string>, and 'Desk.label' holds text or numbers, which no one C# type holds: a door for it is a gap to name",
            Declaring("""    public Property<string> [|Label|] => Property<string>();"""),
            Desk);

    [Test]
    public Task a_live_object_over_a_member_that_holds_a_promise_is_reported_on_the_reaching_declaration()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS025",
            "'ChangedPayload.Pending' is declared with LiveObject<Waiting>, and 'Changed.pending' holds a promise: no door awaits a promise a member holds, a gap to name",
            Declaring(
                """    public Event<ChangedPayload> Changed => Event<ChangedPayload>();""",
                """
                public sealed class ChangedPayload : JsObject
                {
                    public Waiting [|Pending|] => LiveObject<Waiting>();
                }

                public sealed class Waiting : JsObject
                {
                    public ReadOnly<string> State => ReadOnly<string>();
                }
                """),
            Desk);

    [Test]
    public Task a_component_reached_where_its_vendor_hands_a_base_of_the_type_it_mirrors_is_reported_on_the_reaching_declaration()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS021",
            "'ChangedPayload.Target' reaches 'TextBox' at 'Changed.target', which the vendor's TypeScript types as 'EventTarget', not 'Box' that 'TextBox' mirrors: type it there as the object it hands",
            Declaring(
                """    public Event<ChangedPayload> Changed => Event<ChangedPayload>();""",
                """
                public sealed class ChangedPayload : JsObject
                {
                    public TextBox [|Target|] => LiveObject<TextBox>(nullable: true);
                }
                """),
            Desk);

    [Test]
    public Task a_component_reached_where_its_vendor_hands_the_type_it_mirrors_is_not_reported()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Declaring(
                """    public Event<ChangedPayload> Changed => Event<ChangedPayload>();""",
                """
                public sealed class ChangedPayload : JsObject
                {
                    public TextBox Source => LiveObject<TextBox>();
                }
                """),
            Desk);
}
