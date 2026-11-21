using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationMarksAMemberOptional
{
    private const string Tray = """
        type	tray	Tray	class			
        type	tray	Cover	class			
        member	tray	Tray	portion	property	optional	
        member	tray	Tray	room	readonly		
        member	tray	Tray	warm	property	optional,callable	
        member	tray	Tray	fold	method	callable	
        member	tray	Tray	garnish	property	optional,callable	
        member	tray	Tray	cover	readonly	optional	
        member	tray	Tray	lid	property	optional,callable	
        member	tray	Cover	label	readonly		
        member	tray	Cover	tag	readonly	optional	
        holds	tray	Tray	portion	value	other		string
        holds	tray	Tray	room	value	other		string
        holds	tray	Tray	warm	value	function		
        holds	tray	Tray	warm	returns	other		void
        holds	tray	Tray	fold	value	other		void
        holds	tray	Tray	garnish	value	function		
        holds	tray	Tray	garnish	parameter:0	object	tray	Cover
        holds	tray	Tray	garnish	returns	other		string
        holds	tray	Tray	cover	value	object	tray	Cover
        holds	tray	Tray	lid	value	function		
        holds	tray	Tray	lid	returns	object	tray	Cover
        holds	tray	Cover	label	value	other		string
        holds	tray	Cover	tag	value	other		string
        """;

    private const string Cover = """
        public sealed class TrayCover : JsObject
        {
            public ReadOnly<string> Label => ReadOnly<string>();

            public ReadOnly<string> Tag => ReadOnly<string>(optional: true);
        }
        """;

    private static string Declaring(string member, string reached = Cover) => $$"""
        using Alis.Reactive.Components;

        namespace Ward;

        [Mirrors("tray", "Tray")]
        public sealed class MealTray : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{member}}
        }

        {{reached}}
        """;

    [Test]
    public Task each_member_declared_present_or_optional_as_its_vendor_types_it_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""
                public Property<string> Portion => Property<string>(optional: true);

                public ReadOnly<string> Room => ReadOnly<string>();

                public void Warm() => Method(optional: true);

                public void Fold() => Method();

                public Callback<TrayCover, string> Garnish => Callback<TrayCover, string>(optional: true);

                public TrayCover Cover => LiveObject<TrayCover>(optional: true);

                public TrayCover Lid() => ReturnedObject<TrayCover>(optional: true);
            """), Tray);

    [Test]
    public Task a_value_declared_required_that_its_vendor_types_optional_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'MealTray.Portion' declares 'portion' required, and 'Tray.portion' is optional: declare it with optional: true",
            Declaring("""    public Property<string> [|Portion|] => Property<string>();"""),
            Tray);

    [Test]
    public Task a_value_declared_optional_that_its_vendor_types_required_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'MealTray.Room' declares 'room' optional, and 'Tray.room' is required: declare it without optional: true",
            Declaring("""    public ReadOnly<string> [|Room|] => ReadOnly<string>(optional: true);"""),
            Tray);

    [Test]
    public Task a_method_declared_required_that_its_vendor_types_optional_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'MealTray.Warm' declares 'warm' required, and 'Tray.warm' is optional: declare it with optional: true",
            Declaring("""    public void [|Warm|]() => Method();"""),
            Tray);

    [Test]
    public Task a_method_declared_optional_that_its_vendor_types_required_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'MealTray.Fold' declares 'fold' optional, and 'Tray.fold' is required: declare it without optional: true",
            Declaring("""    public void [|Fold|]() => Method(optional: true);"""),
            Tray);

    [Test]
    public Task a_callback_declared_required_that_its_vendor_types_optional_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'MealTray.Garnish' declares 'garnish' required, and 'Tray.garnish' is optional: declare it with optional: true",
            Declaring("""    public Callback<TrayCover, string> [|Garnish|] => Callback<TrayCover, string>();"""),
            Tray);

    [Test]
    public Task a_member_of_a_reached_object_is_checked_at_the_type_it_reaches()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'TrayCover.Tag' declares 'tag' required, and 'Cover.tag' is optional: declare it with optional: true",
            Declaring("""    public TrayCover Cover => LiveObject<TrayCover>(optional: true);""", """
                public sealed class TrayCover : JsObject
                {
                    public ReadOnly<string> Label => ReadOnly<string>();

                    public ReadOnly<string> [|Tag|] => ReadOnly<string>();
                }
                """),
            Tray);

    [Test]
    public Task a_member_of_an_object_a_callable_property_returns_is_checked_at_the_type_it_returns()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS022",
            "'TrayCover.Label' declares 'label' optional, and 'Cover.label' is required: declare it without optional: true",
            Declaring("""    public TrayCover Lid() => ReturnedObject<TrayCover>(optional: true);""", """
                public sealed class TrayCover : JsObject
                {
                    public ReadOnly<string> [|Label|] => ReadOnly<string>(optional: true);

                    public ReadOnly<string> Tag => ReadOnly<string>(optional: true);
                }
                """),
            Tray);
}
