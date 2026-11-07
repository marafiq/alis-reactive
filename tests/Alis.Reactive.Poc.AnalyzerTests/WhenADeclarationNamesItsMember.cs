using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationNamesItsMember
{
    private static string Declaring(string members) => $$"""
        using System;
        using Alis.Reactive.Components;
        using Alis.Reactive.Values;

        namespace Ward;

        public sealed class WardBed : JsObject
        {
            private static readonly bool Older = DateTime.Now.Year > 2000;

            private static string Named() => "room";

        {{members}}
        }
        """;

    private const string Bed = """
        type	ward	Bed	class			
        member	ward	Bed	turn	method	callable	
        member	ward	Bed	label	readonly		
        holds	ward	Bed	turn	value	other		void
        holds	ward	Bed	label	value	other		string
        """;

    private static string Turning(string members) => $$"""
        using Alis.Reactive.Components;

        namespace Ward;

        public interface ITurning
        {
            void Turn();

            ReadOnly<string> Label { get; }

            ReadOnly<string> Title => throw new System.NotSupportedException();
        }

        [Mirrors("ward", "Bed")]
        public sealed class TurningBed : Component, ITurning
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{members}}
        }
        """;

    [Test]
    public Task each_door_that_is_its_members_body_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""
                public ReadOnly<string> Resident => ReadOnly<string>();

                public ReadOnly<string> Room { get { return ReadOnly<string>(name: "room", optional: false); } }

                public ReadOnly<string> Wing => (ReadOnly<string>(name: null));

                public void Turn() { Method(); }

                public MethodResult<string> Chart() => Method<string>();

                public WardBed Partner => LiveObject<WardBed>(nullable: true);

                public ReadOnly<string> Caller => Argument<string>(0);
            """), null);

    [Test]
    public Task a_door_called_in_a_constructor_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls ReadOnly in a constructor, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""    public WardBed() { [|ReadOnly<string>()|]; }"""),
            null);

    [Test]
    public Task a_door_called_in_a_static_member_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls ReadOnly in a static member, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""    public static ReadOnly<string> Shared() => [|new WardBed().ReadOnly<string>()|];"""),
            null);

    [Test]
    public Task a_door_called_in_an_event_accessor_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls Method in an event accessor, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""    public event Action? Turned { add => [|Method()|]; remove { } }"""),
            null);

    [Test]
    public Task a_door_called_in_a_setter_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls ReadOnly in a setter, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""
                private ReadOnly<string>? _resident;

                public ReadOnly<string>? Resident { get => _resident; set => _resident = [|ReadOnly<string>()|]; }
            """),
            null);

    [Test]
    public Task a_door_called_in_an_indexer_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls ReadOnly in an indexer, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""    public ReadOnly<string> this[int bed] => [|ReadOnly<string>()|];"""),
            null);

    [Test]
    public Task a_door_called_in_a_finalizer_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed' calls Method in a finalizer, which declares no JavaScript member: call it as the body of the property or method that declares one",
            Declaring("""    ~WardBed() { [|Method()|]; }"""),
            null);

    [Test]
    public Task an_explicit_interface_member_declares_the_javascript_member_of_its_simple_name()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Turning("""
                void ITurning.Turn() => Method();

                ReadOnly<string> ITurning.Label => ReadOnly<string>();
            """), Bed);

    [Test]
    public Task an_explicit_interface_member_the_vendor_lacks_is_reported_by_its_simple_name()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'TurningBed.Title' declares 'title', which 'Bed' does not make public: declare only what the vendor's typed API makes public",
            Turning("""
                void ITurning.Turn() => Method();

                ReadOnly<string> ITurning.Label => ReadOnly<string>();

                ReadOnly<string> ITurning.[|Title|] => ReadOnly<string>();
            """),
            Bed);

    [Test]
    public Task a_door_whose_result_the_member_reads_further_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed.Letters' calls ReadOnly inside its body: a declaration's body is the one door call that names its JavaScript member",
            Declaring("""    public TypedSource<int> Letters => [|ReadOnly<string>()|].Read(text => text.Length);"""),
            null);

    [Test]
    public Task a_door_called_in_a_lambda_of_its_member_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed.Later' calls ReadOnly inside its body: a declaration's body is the one door call that names its JavaScript member",
            Declaring("""    public Func<ReadOnly<string>> Later => () => [|ReadOnly<string>()|];"""),
            null);

    [Test]
    public Task a_door_beside_another_statement_of_its_member_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed.Turn' calls Method inside its body: a declaration's body is the one door call that names its JavaScript member",
            Declaring("""    public void Turn() { var turns = 1; [|Method()|]; }"""),
            null);

    [Test]
    public Task a_door_inside_another_doors_arguments_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed.Ring' calls Argument inside its body: a declaration's body is the one door call that names its JavaScript member",
            Declaring("""    public void Ring() => Method([(Arg<string>)[|Argument<string>(0)|]]);"""),
            null);

    [Test]
    public Task a_declared_by_the_developer_writes_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS023",
            "'WardBed.Resident' passes declaredBy: to ReadOnly: the member that calls it is the member it declares, and name: names a JavaScript member its C# name does not",
            Declaring("""    public ReadOnly<string> Resident => ReadOnly<string>([|declaredBy: "Room"|]);"""),
            null);

    [Test]
    public Task a_name_the_build_cannot_read_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS024",
            "'WardBed.Resident' passes name: to ReadOnly as a value the build cannot read: pass a constant",
            Declaring("""    public ReadOnly<string> Resident => ReadOnly<string>([|name: Named()|]);"""),
            null);

    [Test]
    public Task an_optional_mark_the_build_cannot_read_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS024",
            "'WardBed.Resident' passes optional: to ReadOnly as a value the build cannot read: pass a constant",
            Declaring("""    public ReadOnly<string> Resident => ReadOnly<string>([|optional: Older|]);"""),
            null);

    [Test]
    public Task a_nullable_mark_the_build_cannot_read_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS024",
            "'WardBed.Partner' passes nullable: to LiveObject as a value the build cannot read: pass a constant",
            Declaring("""    public WardBed Partner => LiveObject<WardBed>([|nullable: Older|]);"""),
            null);

    [Test]
    public Task a_position_the_build_cannot_read_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS024",
            "'WardBed.Caller' passes position: to Argument as a value the build cannot read: pass a constant",
            Declaring("""    public ReadOnly<string> Caller => Argument<string>([|Older ? 1 : 0|]);"""),
            null);
}
