using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationReachesAnObject
{
    private const string Station = """
        type	ward	Station	class			
        type	ward	StationEvents	interface			
        type	ward	Bed	class			
        type	ward	OlderBed	class			
        type	ward	Ring	interface			
        type	ward	Alarm	interface			
        type	ward	Paged	interface			
        member	ward	Station	beds	readonly		
        member	ward	Station	lead	readonly		
        member	ward	Station	leadOrNone	readonly	readsnull	
        member	ward	Station	bedsOrNone	readonly	readsnull	
        member	ward	Station	findOrNone	method	callable,returnsnull	
        member	ward	Station	either	readonly		
        member	ward	Station	older	readonly		
        member	ward	Station	count	readonly		
        member	ward	Station	find	method	callable	
        member	ward	Station	ring	property	readsnull,writesnull,callable	
        member	ward	Station	label	property	callable	
        member	ward	Station	on	method	callable	name:alarm,name:noise
        member	ward	Station	addEventListener	method	callable	keys:ward:StationEvents
        member	ward	StationEvents	paged	readonly		
        member	ward	Bed	resident	readonly		
        member	ward	Bed	occupied	readonly		
        member	ward	OlderBed	resident	readonly		
        member	ward	OlderBed	partner	method	callable	
        member	ward	Ring	room	readonly		
        member	ward	Alarm	bed	readonly		
        member	ward	Paged	by	readonly		
        holds	ward	Station	beds	value	items	ward	Bed
        holds	ward	Station	lead	value	object	ward	Bed
        holds	ward	Station	leadOrNone	value	null		
        holds	ward	Station	leadOrNone	value	object	ward	Bed
        holds	ward	Station	bedsOrNone	value	items	ward	Bed
        holds	ward	Station	bedsOrNone	value	null		
        holds	ward	Station	findOrNone	value	null		
        holds	ward	Station	findOrNone	value	object	ward	Bed
        holds	ward	Station	either	value	object	ward	Bed
        holds	ward	Station	either	value	object	ward	OlderBed
        holds	ward	Station	older	value	object	ward	OlderBed
        holds	ward	Station	count	value	other		number
        holds	ward	Station	find	value	object	ward	Bed
        holds	ward	Station	ring	value	function		
        holds	ward	Station	ring	value	null		
        holds	ward	Station	ring	parameter:0	object	ward	Ring
        holds	ward	Station	ring	parameter:1	null		
        holds	ward	Station	ring	parameter:1	other		string
        holds	ward	Station	label	value	function		
        holds	ward	Station	label	parameter:0	object	ward	Bed
        holds	ward	Station	label	parameter:1	function		
        holds	ward	Station	on	value	other		void
        holds	ward	Station	on	event:alarm:parameter:0	object	ward	Alarm
        holds	ward	Station	on	event:noise:rest:0	null		
        holds	ward	Station	on	event:noise:rest:0	other		unknown
        holds	ward	StationEvents	paged	value	object	ward	Paged
        holds	ward	OlderBed	partner	value	self		
        """;

    private static string Reaching(string member, string reached = "") => $$"""
        using System.Diagnostics.CodeAnalysis;
        using Alis.Reactive.Components;

        namespace Ward;

        [Mirrors("ward", "Station")]
        public sealed class WardStation : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{member}}
        }

        public sealed class WardBed : JsObject
        {
            public ReadOnly<string> Resident => ReadOnly<string>();
        }

        {{reached}}
        """;

    private const string BedOccupied = """
        public sealed class OccupiedBed : JsObject
        {
            public ReadOnly<string> Resident => ReadOnly<string>();

            public ReadOnly<bool> [|Occupied|] => ReadOnly<bool>();
        }
        """;

    private const string BedFloor = """
        public sealed class FloorBed : JsObject
        {
            public ReadOnly<string> Resident => ReadOnly<string>();

            public ReadOnly<string> [|Floor|] => ReadOnly<string>();
        }
        """;

    private const string FloorAtBed = "'FloorBed.Floor' declares 'floor', which 'Bed' does not make public: declare only what the vendor's typed API makes public";

    [Test]
    public Task a_live_object_a_member_holds_is_reported_by_none_when_it_declares_what_that_type_makes_public()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Reaching("""    public WardBed Lead => LiveObject<WardBed>();"""), Station);

    [Test]
    public Task a_live_object_that_declares_a_member_the_held_type_lacks_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'OccupiedBed.Occupied' declares 'occupied', which 'OlderBed' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public OccupiedBed Older => LiveObject<OccupiedBed>();""", BedOccupied),
            Station);

    [Test]
    public Task a_declaration_no_root_reaches_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Reaching("""    public WardBed Lead => LiveObject<WardBed>();""", BedFloor.Replace("[|", "").Replace("|]", "")), Station);

    [Test]
    public Task each_item_of_a_list_is_checked_at_the_element_type()
        => Fixture.Reports<MirrorsAnalyzer>("ALIS016", FloorAtBed, Reaching("""    public LiveObjects<FloorBed> Beds => LiveObjects<FloorBed>();""", BedFloor), Station);

    [Test]
    public Task a_union_is_checked_at_each_type_it_holds()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'OccupiedBed.Occupied' declares 'occupied', which 'OlderBed' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public OccupiedBed Either => LiveObject<OccupiedBed>();""", BedOccupied),
            Station);

    [Test]
    public Task a_member_that_can_hold_nothing_reaches_the_object_it_holds_otherwise()
        => Fixture.Reports<MirrorsAnalyzer>("ALIS016", FloorAtBed, Reaching("""    public FloorBed LeadOrNone => LiveObject<FloorBed>(nullable: true);""", BedFloor), Station);

    [Test]
    public Task a_live_object_its_vendor_can_hold_as_null_is_reported_unless_declared_nullable()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardStation.LeadOrNone' reaches 'WardBed', and 'Station.leadOrNone' can read null or undefined: declare it with nullable: true",
            Reaching("""    public WardBed [|LeadOrNone|] => LiveObject<WardBed>();"""),
            Station);

    [Test]
    public Task a_list_its_vendor_can_hold_as_null_is_reported_unless_declared_nullable()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardStation.BedsOrNone' reaches 'WardBed', and 'Station.bedsOrNone' can read null or undefined: declare it with nullable: true",
            Reaching("""    public LiveObjects<WardBed> [|BedsOrNone|] => LiveObjects<WardBed>();"""),
            Station);

    [Test]
    public Task a_returned_object_its_vendor_can_return_as_null_is_reported_unless_declared_nullable()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardStation.FindOrNone' reaches 'WardBed', and 'Station.findOrNone' can return null or undefined: declare it with nullable: true",
            Reaching("""    public WardBed [|FindOrNone|]() => ReturnedObject<WardBed>();"""),
            Station);

    [Test]
    public Task a_live_object_declared_nullable_where_its_vendor_holds_no_null_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Reaching("""    public WardBed Lead => LiveObject<WardBed>(nullable: true);"""), Station);

    [Test]
    public Task a_returned_object_is_checked_at_the_type_the_method_returns()
        => Fixture.Reports<MirrorsAnalyzer>("ALIS016", FloorAtBed, Reaching("""    public FloorBed Find() => ReturnedObject<FloorBed>();""", BedFloor), Station);

    [Test]
    public Task an_object_a_method_returns_as_itself_is_checked_at_the_type_it_is_read_on_once()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'PairedBed.Occupied' declares 'occupied', which 'OlderBed' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public PairedBed Older => LiveObject<PairedBed>();""", """
                public sealed class PairedBed : JsObject
                {
                    public ReadOnly<bool> [|Occupied|] => ReadOnly<bool>();

                    public PairedBed Partner() => ReturnedObject<PairedBed>();
                }
                """),
            Station);

    [Test]
    public Task one_object_declared_where_a_primitive_is_held_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS021",
            "'WardStation.Count' reaches 'WardBed' at 'Station.count', which the vendor's TypeScript types as 'number': type it there as the object it hands",
            Reaching("""    public WardBed [|Count|] => LiveObject<WardBed>();"""),
            Station);

    [Test]
    public Task one_object_declared_where_a_list_is_held_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS021",
            "'WardStation.Beds' reaches one 'WardBed' at 'Station.beds', which the vendor's TypeScript types as a list of 'Bed': declare it with LiveObjects",
            Reaching("""    public WardBed [|Beds|] => LiveObject<WardBed>();"""),
            Station);

    [Test]
    public Task a_list_declared_where_one_object_is_held_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS021",
            "'WardStation.Lead' reaches a list of 'WardBed' at 'Station.lead', which the vendor's TypeScript types as 'Bed', no list: declare it with LiveObject, or type it there as the list it hands",
            Reaching("""    public LiveObjects<WardBed> [|Lead|] => LiveObjects<WardBed>();"""),
            Station);

    [Test]
    public Task a_payload_a_callable_property_hands_is_checked_at_its_first_parameter()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'RingPayload.Floor' declares 'floor', which 'Ring' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public Event<RingPayload> Ring => Event<RingPayload>();""", """
                public sealed class RingPayload : JsObject
                {
                    public ReadOnly<string> Room => ReadOnly<string>();

                    public ReadOnly<string> [|Floor|] => ReadOnly<string>();
                }
                """),
            Station);

    [Test]
    public Task a_payload_a_subscribe_method_hands_for_a_literal_is_checked_at_its_listeners_first_parameter()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'AlarmPayload.Room' declares 'room', which 'Alarm' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public Event<AlarmPayload> Alarm => Event<AlarmPayload>();""", """
                public sealed class AlarmPayload : JsObject
                {
                    public ReadOnly<string> Bed => ReadOnly<string>();

                    public ReadOnly<string> [|Room|] => ReadOnly<string>();
                }
                """),
            Station);

    [Test]
    public Task a_payload_an_event_map_hands_is_checked_at_the_maps_member()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'PagedPayload.Bed' declares 'bed', which 'Paged' does not make public: declare only what the vendor's typed API makes public",
            Reaching("""    public Event<PagedPayload> Paged => Event<PagedPayload>();""", """
                public sealed class PagedPayload : JsObject
                {
                    public ReadOnly<string> By => ReadOnly<string>();

                    public ReadOnly<string> [|Bed|] => ReadOnly<string>();
                }
                """),
            Station);

    [Test]
    public Task a_payload_read_by_its_members_where_the_listener_takes_unknown_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS021",
            "'WardStation.Noise' reaches 'NoisePayload' at the first parameter of the listener of 'Station' event 'noise', which the vendor's TypeScript types as 'unknown': type it there as the object it hands",
            Reaching("""    public Event<NoisePayload> [|Noise|] => Event<NoisePayload>();""", """
                public sealed class NoisePayload : JsObject
                {
                    public ReadOnly<string?> Level => ReadOnly<string?>();
                }
                """),
            Station);

    [Test]
    public Task a_payload_read_only_by_position_is_checked_against_the_listeners_parameters()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Reaching("""    public Event<NoiseArguments> Noise => Event<NoiseArguments>();""", """
                public sealed class NoiseArguments : JsObject
                {
                    public ReadOnly<string?> Third => Argument<string?>(3);
                }
                """),
            Station);

    [Test]
    public Task an_argument_read_as_never_null_where_the_listener_can_hand_null_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'RingArguments.Floor' reads 'string' at argument 1, and the listener of 'Station' event 'ring' can hand null or undefined there: declare 'string?'",
            Reaching("""    public Event<RingArguments> Ring => Event<RingArguments>();""", """
                public sealed class RingArguments : JsObject
                {
                    public ReadOnly<string> [|Floor|] => Argument<string>(1);
                }
                """),
            Station);

    [Test]
    public Task an_argument_past_the_listeners_parameters_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS016",
            "'RingArguments.Wing' is declared with Argument(2), and the listener of 'Station' event 'ring' hands no argument at 2: declare only the arguments the vendor's typed API hands",
            Reaching("""    public Event<RingArguments> Ring => Event<RingArguments>();""", """
                public sealed class RingArguments : JsObject
                {
                    public ReadOnly<string?> Floor => Argument<string?>(1);

                    public ReadOnly<string?> [|Wing|] => Argument<string?>(2);
                }
                """),
            Station);

    [Test]
    public Task a_method_at_a_position_the_vendor_hands_a_function_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Reaching("""    public Callback<LabelArguments, string> Label => Callback<LabelArguments, string>();""", """
                public sealed class LabelArguments : JsObject
                {
                    public void Done() => MethodAt(1);
                }
                """),
            Station);

    [Test]
    public Task a_method_at_a_position_the_vendor_hands_no_function_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'LabelArguments.Done' is declared with MethodAt(0), and 'Station.label' hands 'Bed' at 0: type it there as the function it is",
            Reaching("""    public Callback<LabelArguments, string> Label => Callback<LabelArguments, string>();""", """
                public sealed class LabelArguments : JsObject
                {
                    public void [|Done|]() => MethodAt(0);
                }
                """),
            Station);

    [Test]
    public Task a_callback_that_answers_null_where_the_vendor_takes_none_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS019",
            "'WardStation.Label' answers 'string?', and 'Station.label' returns no null: declare 'string'",
            Reaching("""    public Callback<WardBed, string?> [|Label|] => Callback<WardBed, string?>();"""),
            Station);

    [Test]
    public Task an_argument_on_a_component_is_reported_where_it_is_declared()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'WardStation.First' is declared with Argument(0), and 'WardStation' is handed no arguments: declare it on the payload an event hands its pipeline, or on the arguments a callback receives",
            Reaching("""    public ReadOnly<string> [|First|] => Argument<string>(0);"""),
            Station);

    [Test]
    public Task an_argument_on_an_object_a_member_holds_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS018",
            "'HeldBed.First' is declared with Argument(0), and 'HeldBed' is handed no arguments: declare it on the payload an event hands its pipeline, or on the arguments a callback receives",
            Reaching("""    public HeldBed Lead => LiveObject<HeldBed>();""", """
                public sealed class HeldBed : JsObject
                {
                    public ReadOnly<string> [|First|] => Argument<string>(0);
                }
                """),
            Station);

    [Test]
    public Task a_drift_suppressed_on_a_reached_member_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(
            Reaching("""    public OccupiedBed Older => LiveObject<OccupiedBed>();""", """
                public sealed class OccupiedBed : JsObject
                {
                    [SuppressMessage("Alis.Reactive.Contract", "ALIS016", Justification = "Drift on purpose.")]
                    public ReadOnly<bool> Occupied => ReadOnly<bool>();
                }
                """),
            Station);
}
