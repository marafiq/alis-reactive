using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADeclarationDeclaresAnEvent
{
    private const string Feed = """
        type	ward	Feed	class		ward:Hub	
        type	ward	Hub	class			
        type	ward	FeedEvents	interface		ward:HubEvents	
        type	ward	HubEvents	interface			
        member	ward	FeedEvents	update	readonly		
        member	ward	HubEvents	closed	readonly		
        member	ward	Feed	onring	property	readsnull,writesnull,callable	
        member	ward	Feed	title	property		
        member	ward	Feed	refresh	method	callable	
        member	ward	Feed	on	method	callable	name:paged,name:silenced
        member	ward	Feed	addEventListener	method	callable	keys:ward:FeedEvents
        member	ward	Hub	subscribe	method	callable	name:opened
        """;

    private static string Declaring(string member) => $$"""
        using Alis.Reactive.Components;

        namespace Ward;

        [Mirrors("ward", "Feed")]
        public sealed class WardFeed : Component
        {
            public override ComponentVendor Vendor => ComponentVendor.Native;

        {{member}}
        }
        """;

    [Test]
    public Task an_event_a_property_holding_a_function_declares_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""    public Event Ring => Event(name: "onring");"""), Feed);

    [Test]
    public Task an_event_named_as_a_property_that_holds_no_function_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS020",
            "'WardFeed.Title' declares the event 'title', which 'Feed' does not declare: a vendor types an event as a property of its name that holds a function, or as a literal its subscribe method takes, as lib.dom's addEventListener takes the keys of HTMLElementEventMap",
            Declaring("""    public Event [|Title|] => Event();"""),
            Feed);

    [Test]
    public Task an_event_named_as_a_method_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS020",
            "'WardFeed.Refresh' declares the event 'refresh', which 'Feed' does not declare: a vendor types an event as a property of its name that holds a function, or as a literal its subscribe method takes, as lib.dom's addEventListener takes the keys of HTMLElementEventMap",
            Declaring("""    public Event [|Refresh|] => Event();"""),
            Feed);

    [Test]
    public Task an_event_a_literal_of_a_subscribe_method_names_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""    public Event Paged => Event();"""), Feed);

    [Test]
    public Task an_event_no_subscribe_method_names_is_reported()
        => Fixture.Reports<MirrorsAnalyzer>(
            "ALIS020",
            "'WardFeed.Alarm' declares the event 'alarm', which 'Feed' does not declare: a vendor types an event as a property of its name that holds a function, or as a literal its subscribe method takes, as lib.dom's addEventListener takes the keys of HTMLElementEventMap",
            Declaring("""    public Event [|Alarm|] => Event();"""),
            Feed);

    [Test]
    public Task an_event_a_key_of_the_event_map_names_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""    public Event Update => Event();"""), Feed);

    [Test]
    public Task an_event_a_base_of_the_event_map_names_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""    public Event Closed => Event();"""), Feed);

    [Test]
    public Task an_event_a_base_types_subscribe_method_names_is_reported_by_none()
        => Fixture.ReportsNothing<MirrorsAnalyzer>(Declaring("""    public Event Opened => Event();"""), Feed);

    [Test]
    public Task a_type_a_second_contract_file_lists_is_found()
        => Fixture.ReportsNothingWith<MirrorsAnalyzer>(
            Declaring("""    public Event Paged => Event();"""),
            ["type\tlib.dom\tHTMLElement\tinterface\t\t\t\n", Feed]);

    [Test]
    public Task a_type_two_contract_files_list_is_reported_as_listed_twice()
        => Fixture.ReportsWith<MirrorsAnalyzer>(
            "ALIS015",
            "'WardFeed' mirrors 'Feed' of 'ward', which the vendor contract lists more than once",
            """
            using Alis.Reactive.Components;

            namespace Ward;

            [[|Mirrors("ward", "Feed")|]]
            public sealed class WardFeed : Component
            {
                public override ComponentVendor Vendor => ComponentVendor.Native;
            }
            """,
            [Feed, Feed]);
}
