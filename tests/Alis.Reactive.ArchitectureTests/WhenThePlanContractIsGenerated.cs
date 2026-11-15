using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PlanContractGenerator;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenThePlanContractIsGenerated
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    private static readonly WrittenType[] ABedIsWrittenAsItsNumber =
        [new(typeof(Bed), "export type Bed = number;", new JsonObject { ["type"] = "integer" }, kinds: [], uses: [])];

    private static readonly IReadOnlyDictionary<Type, IReadOnlyList<Type>> Contracts = new Dictionary<Type, IReadOnlyList<Type>>
    {
        [typeof(DayVisit)] = [typeof(VisitReport)],
        [typeof(Breakfast)] = [typeof(OpenRound)],
    };

    [AcceptedCase]
    public void a_closed_grammar_is_written_as_its_typescript()
        => Assert.That(PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber, Contracts).TypeScript(), Is.EqualTo(
            """
            export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };

            export interface Round {
              readonly version: 3;
              readonly ward: string;
              readonly shift: Shift;
              readonly visits: readonly Visit[];
              readonly free: readonly Bed[];
              readonly cleaned: Readonly<Record<string, boolean>>;
              readonly notes: JsonValue;
            }

            export type Shift = "early" | "late";

            export type Visit = DayVisit | NightVisit;

            export interface DayVisit {
              readonly kind: "day";
              readonly nurse: string;
              readonly beds: number;
            }

            export interface NightVisit {
              readonly kind: "night";
            }

            export type Bed = number;

            export interface VisitReport {
              readonly result: Result;
              readonly bytes: number;
              readonly shift: Shift;
            }

            export type Result = DayResult | NightResult;

            export interface DayResult {
              readonly kind: "day";
              readonly nurse: string;
            }

            export interface NightResult {
              readonly kind: "night";
            }

            """));

    [AcceptedCase]
    public void a_closed_grammar_is_written_as_its_schema()
        => Assert.That(PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber, Contracts).Schema(), Is.EqualTo(
            """
            {
              "$schema": "https://json-schema.org/draft/2020-12/schema",
              "$ref": "#/$defs/Round",
              "$defs": {
                "Round": {
                  "type": "object",
                  "properties": {
                    "version": {
                      "const": 3
                    },
                    "ward": {
                      "type": "string"
                    },
                    "shift": {
                      "$ref": "#/$defs/Shift"
                    },
                    "visits": {
                      "type": "array",
                      "items": {
                        "$ref": "#/$defs/Visit"
                      }
                    },
                    "free": {
                      "type": "array",
                      "items": {
                        "$ref": "#/$defs/Bed"
                      }
                    },
                    "cleaned": {
                      "type": "object",
                      "additionalProperties": {
                        "type": "boolean"
                      }
                    },
                    "notes": {}
                  },
                  "required": [
                    "version",
                    "ward",
                    "shift",
                    "visits",
                    "free",
                    "cleaned",
                    "notes"
                  ],
                  "additionalProperties": false
                },
                "Shift": {
                  "enum": [
                    "early",
                    "late"
                  ]
                },
                "Visit": {
                  "oneOf": [
                    {
                      "$ref": "#/$defs/DayVisit"
                    },
                    {
                      "$ref": "#/$defs/NightVisit"
                    }
                  ]
                },
                "DayVisit": {
                  "type": "object",
                  "properties": {
                    "kind": {
                      "const": "day"
                    },
                    "nurse": {
                      "type": "string"
                    },
                    "beds": {
                      "type": "integer"
                    }
                  },
                  "required": [
                    "kind",
                    "nurse",
                    "beds"
                  ],
                  "additionalProperties": false
                },
                "NightVisit": {
                  "type": "object",
                  "properties": {
                    "kind": {
                      "const": "night"
                    }
                  },
                  "required": [
                    "kind"
                  ],
                  "additionalProperties": false
                },
                "Bed": {
                  "type": "integer"
                },
                "VisitReport": {
                  "type": "object",
                  "properties": {
                    "result": {
                      "$ref": "#/$defs/Result"
                    },
                    "bytes": {
                      "type": "integer"
                    },
                    "shift": {
                      "$ref": "#/$defs/Shift"
                    }
                  },
                  "required": [
                    "result",
                    "bytes",
                    "shift"
                  ],
                  "additionalProperties": false
                },
                "Result": {
                  "oneOf": [
                    {
                      "$ref": "#/$defs/DayResult"
                    },
                    {
                      "$ref": "#/$defs/NightResult"
                    }
                  ]
                },
                "DayResult": {
                  "type": "object",
                  "properties": {
                    "kind": {
                      "const": "day"
                    },
                    "nurse": {
                      "type": "string"
                    }
                  },
                  "required": [
                    "kind",
                    "nurse"
                  ],
                  "additionalProperties": false
                },
                "NightResult": {
                  "type": "object",
                  "properties": {
                    "kind": {
                      "const": "night"
                    }
                  },
                  "required": [
                    "kind"
                  ],
                  "additionalProperties": false
                }
              }
            }

            """));

    [AcceptedCase]
    public void the_grammar_reaches_every_request_contract_and_part()
    {
        var definitions = JsonNode.Parse(PlanContract.ReadCore().Schema())!["$defs"]!.AsObject();
        var contracts = definitions.SkipWhile(row => row.Key is not ("HttpSend" or "FormCheck"))
            .OrderBy(row => row.Key, StringComparer.Ordinal)
            .Select(row => row.Key + "=" + row.Value!.ToJsonString());
        Assert.That(string.Join("\n", contracts), Is.EqualTo(
            """
            Antiforgery={"oneOf":[{"$ref":"#/$defs/NoneAntiforgery"},{"$ref":"#/$defs/TokenAntiforgery"}]}
            BodyFormat={"enum":["none","json","urlEncoded","multipart","text","file"]}
            BooleanScalarWireForm={"type":"object","properties":{"kind":{"const":"boolean"}},"required":["kind"],"additionalProperties":false}
            BrowserHeaders={}
            Busy={"oneOf":[{"$ref":"#/$defs/NoneBusy"},{"$ref":"#/$defs/PageBusy"},{"$ref":"#/$defs/TargetsBusy"}]}
            BusyTarget={"oneOf":[{"$ref":"#/$defs/MissingBusyTarget"},{"$ref":"#/$defs/ElementBusyTarget"},{"$ref":"#/$defs/ComponentBusyTarget"}]}
            CapturedContainerCapture={"type":"object","properties":{"kind":{"const":"captured"},"id":{"type":"string"},"body":{"$ref":"#/$defs/EvaluatedValue"},"inputs":{"type":"array","items":{"$ref":"#/$defs/SentInput"}}},"required":["kind","id","body","inputs"],"additionalProperties":false}
            CapturedInputCapture={"type":"object","properties":{"kind":{"const":"captured"},"input":{"$ref":"#/$defs/SentInput"}},"required":["kind","input"],"additionalProperties":false}
            CheckedBody={"type":"object","properties":{"format":{"$ref":"#/$defs/BodyFormat"},"value":{"$ref":"#/$defs/EvaluatedValue"}},"required":["format","value"],"additionalProperties":false}
            ComponentBusyTarget={"type":"object","properties":{"kind":{"const":"component"},"value":{"$ref":"#/$defs/ValueRead"}},"required":["kind","value"],"additionalProperties":false}
            ContainerCapture={"oneOf":[{"$ref":"#/$defs/NoneContainerCapture"},{"$ref":"#/$defs/MissingContainerCapture"},{"$ref":"#/$defs/CapturedContainerCapture"}]}
            DateScalarWireForm={"type":"object","properties":{"kind":{"const":"date"}},"required":["kind"],"additionalProperties":false}
            ElementBusyTarget={"type":"object","properties":{"kind":{"const":"element"},"id":{"type":"string"}},"required":["kind","id"],"additionalProperties":false}
            EnumScalarWireForm={"type":"object","properties":{"kind":{"const":"enum"},"values":{"type":"array","items":{"$ref":"#/$defs/EnumValue"}}},"required":["kind","values"],"additionalProperties":false}
            EnumValue={"type":"object","properties":{"held":{},"name":{"type":"string"}},"required":["held","name"],"additionalProperties":false}
            EvaluatedValue={}
            FieldMultipartPart={"type":"object","properties":{"kind":{"const":"field"},"field":{"$ref":"#/$defs/SentField"}},"required":["kind","field"],"additionalProperties":false}
            FieldTokenPlace={"type":"object","properties":{"kind":{"const":"field"},"name":{"type":"string"}},"required":["kind","name"],"additionalProperties":false}
            FileMultipartPart={"type":"object","properties":{"kind":{"const":"file"},"name":{"type":"string"},"value":{"$ref":"#/$defs/ValueRead"}},"required":["kind","name","value"],"additionalProperties":false}
            FileResponseBody={"type":"object","properties":{"kind":{"const":"file"},"value":{"$ref":"#/$defs/ResponseFile"}},"required":["kind","value"],"additionalProperties":false}
            FileSentBody={"type":"object","properties":{"kind":{"const":"file"},"value":{"$ref":"#/$defs/ValueRead"},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["kind","value","inputs"],"additionalProperties":false}
            FlagValue={"type":"object","properties":{"held":{},"name":{"type":"string"},"number":{"type":"integer"}},"required":["held","name","number"],"additionalProperties":false}
            FlagsScalarWireForm={"type":"object","properties":{"kind":{"const":"flags"},"values":{"type":"array","items":{"$ref":"#/$defs/FlagValue"}}},"required":["kind","values"],"additionalProperties":false}
            FormCheck={"type":"object","properties":{"registration":{"type":"string"},"body":{"$ref":"#/$defs/CheckedBody"},"inputs":{"type":"array","items":{"$ref":"#/$defs/SentInput"}},"place":{"$ref":"#/$defs/FormPlace"}},"required":["registration","body","inputs","place"],"additionalProperties":false}
            FormDestination={"oneOf":[{"$ref":"#/$defs/SummaryFormDestination"},{"$ref":"#/$defs/NoticeFormDestination"}]}
            FormPlace={"type":"object","properties":{"destination":{"$ref":"#/$defs/FormDestination"}},"required":["destination"],"additionalProperties":false}
            HeaderTokenPlace={"type":"object","properties":{"kind":{"const":"header"},"name":{"type":"string"}},"required":["kind","name"],"additionalProperties":false}
            HeldValueRead={"type":"object","properties":{"kind":{"const":"held"},"value":{"$ref":"#/$defs/EvaluatedValue"}},"required":["kind","value"],"additionalProperties":false}
            HtmlResponseBody={"type":"object","properties":{"kind":{"const":"html"},"value":{"type":"string"}},"required":["kind","value"],"additionalProperties":false}
            HttpOutcome={"enum":["ok","status","network","redirect","content"]}
            HttpResponse={"type":"object","properties":{"status":{"type":"integer"},"outcome":{"$ref":"#/$defs/HttpOutcome"},"headers":{"$ref":"#/$defs/BrowserHeaders"},"request":{"$ref":"#/$defs/EvaluatedValue"},"inputs":{"type":"array","items":{"$ref":"#/$defs/SentInput"}},"body":{"$ref":"#/$defs/ResponseBody"}},"required":["status","outcome","headers","request","inputs","body"],"additionalProperties":false}
            HttpSend={"type":"object","properties":{"method":{"$ref":"#/$defs/RequestMethod"},"url":{"$ref":"#/$defs/RequestUrl"},"query":{"$ref":"#/$defs/SentQuery"},"body":{"$ref":"#/$defs/SentBody"},"headers":{"type":"array","items":{"$ref":"#/$defs/SentHeader"}},"antiforgery":{"$ref":"#/$defs/Antiforgery"},"retry":{"$ref":"#/$defs/RetryPolicy"},"busy":{"$ref":"#/$defs/Busy"},"respondedJson":{"type":"boolean"},"contracts":{"type":"array","items":{"$ref":"#/$defs/RequestContract"}}},"required":["method","url","query","body","headers","antiforgery","retry","busy","respondedJson","contracts"],"additionalProperties":false}
            InputCapture={"oneOf":[{"$ref":"#/$defs/MissingInputCapture"},{"$ref":"#/$defs/CapturedInputCapture"}]}
            InputEntry={"type":"object","properties":{"id":{"type":"string"},"binding":{"$ref":"#/$defs/InputBinding"}},"required":["id","binding"],"additionalProperties":false}
            InstantScalarWireForm={"type":"object","properties":{"kind":{"const":"instant"}},"required":["kind"],"additionalProperties":false}
            ItemWhenNothing={"enum":["empty","stop"]}
            JsonResponseBody={"type":"object","properties":{"kind":{"const":"json"},"value":{"$ref":"#/$defs/EvaluatedValue"}},"required":["kind","value"],"additionalProperties":false}
            JsonSentBody={"type":"object","properties":{"kind":{"const":"json"},"value":{"$ref":"#/$defs/EvaluatedValue"},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["kind","value","inputs"],"additionalProperties":false}
            ListWireForm={"type":"object","properties":{"kind":{"const":"list"},"item":{"$ref":"#/$defs/ScalarWireForm"},"itemWhenNothing":{"$ref":"#/$defs/ItemWhenNothing"}},"required":["kind","item","itemWhenNothing"],"additionalProperties":false}
            MemberPathPart={"type":"object","properties":{"kind":{"const":"member"},"name":{"type":"string"},"value":{"$ref":"#/$defs/ValueRead"},"form":{"$ref":"#/$defs/ScalarWireForm"}},"required":["kind","name","value","form"],"additionalProperties":false}
            MissingBusyTarget={"type":"object","properties":{"kind":{"const":"missing"},"id":{"type":"string"}},"required":["kind","id"],"additionalProperties":false}
            MissingContainerCapture={"type":"object","properties":{"kind":{"const":"missing"},"id":{"type":"string"}},"required":["kind","id"],"additionalProperties":false}
            MissingInputCapture={"type":"object","properties":{"kind":{"const":"missing"},"id":{"type":"string"},"name":{"type":"string"}},"required":["kind","id","name"],"additionalProperties":false}
            MultipartPart={"oneOf":[{"$ref":"#/$defs/FieldMultipartPart"},{"$ref":"#/$defs/FileMultipartPart"}]}
            MultipartSentBody={"type":"object","properties":{"kind":{"const":"multipart"},"parts":{"type":"array","items":{"$ref":"#/$defs/MultipartPart"}},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["kind","parts","inputs"],"additionalProperties":false}
            NoneAntiforgery={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NoneBusy={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NoneContainerCapture={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NoneResponseBody={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NoneRetryPolicy={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NoneSentBody={"type":"object","properties":{"kind":{"const":"none"}},"required":["kind"],"additionalProperties":false}
            NothingValueRead={"type":"object","properties":{"kind":{"const":"nothing"}},"required":["kind"],"additionalProperties":false}
            NoticeFormDestination={"type":"object","properties":{"kind":{"const":"notice"},"separator":{"type":"string"}},"required":["kind","separator"],"additionalProperties":false}
            NumberScalarWireForm={"type":"object","properties":{"kind":{"const":"number"}},"required":["kind"],"additionalProperties":false}
            PageBusy={"type":"object","properties":{"kind":{"const":"page"}},"required":["kind"],"additionalProperties":false}
            PathPart={"oneOf":[{"$ref":"#/$defs/TextPathPart"},{"$ref":"#/$defs/MemberPathPart"}]}
            RequestContract={"enum":["busy","notice"]}
            RequestMethod={"enum":["GET","HEAD","OPTIONS","POST","PUT","DELETE"]}
            RequestUrl={"type":"object","properties":{"parts":{"type":"array","items":{"$ref":"#/$defs/PathPart"}},"query":{"type":"string"}},"required":["parts","query"],"additionalProperties":false}
            ResponseBody={"oneOf":[{"$ref":"#/$defs/NoneResponseBody"},{"$ref":"#/$defs/JsonResponseBody"},{"$ref":"#/$defs/TextResponseBody"},{"$ref":"#/$defs/HtmlResponseBody"},{"$ref":"#/$defs/FileResponseBody"}]}
            ResponseFile={"type":"object","properties":{"name":{"type":"string"},"size":{"type":"integer"},"type":{"type":"string"}},"required":["name","size","type"],"additionalProperties":false}
            RetryPolicy={"oneOf":[{"$ref":"#/$defs/NoneRetryPolicy"},{"$ref":"#/$defs/StandardRetryPolicy"}]}
            ScalarWireForm={"oneOf":[{"$ref":"#/$defs/TextScalarWireForm"},{"$ref":"#/$defs/NumberScalarWireForm"},{"$ref":"#/$defs/BooleanScalarWireForm"},{"$ref":"#/$defs/DateScalarWireForm"},{"$ref":"#/$defs/InstantScalarWireForm"},{"$ref":"#/$defs/WrittenScalarWireForm"},{"$ref":"#/$defs/EnumScalarWireForm"},{"$ref":"#/$defs/FlagsScalarWireForm"}]}
            SentBody={"oneOf":[{"$ref":"#/$defs/NoneSentBody"},{"$ref":"#/$defs/JsonSentBody"},{"$ref":"#/$defs/UrlEncodedSentBody"},{"$ref":"#/$defs/MultipartSentBody"},{"$ref":"#/$defs/TextSentBody"},{"$ref":"#/$defs/FileSentBody"}]}
            SentField={"type":"object","properties":{"name":{"type":"string"},"path":{"type":"array","items":{"$ref":"#/$defs/PathStep"}},"value":{"$ref":"#/$defs/ValueRead"},"form":{"$ref":"#/$defs/WireForm"},"whenNothing":{"$ref":"#/$defs/WhenNothing"}},"required":["name","path","value","form","whenNothing"],"additionalProperties":false}
            SentHeader={"type":"object","properties":{"name":{"type":"string"},"value":{"$ref":"#/$defs/ValueRead"},"form":{"$ref":"#/$defs/ScalarWireForm"}},"required":["name","value","form"],"additionalProperties":false}
            SentInput={"type":"object","properties":{"name":{"type":"string"},"entry":{"$ref":"#/$defs/InputEntry"}},"required":["name","entry"],"additionalProperties":false}
            SentInputs={"type":"object","properties":{"container":{"$ref":"#/$defs/ContainerCapture"},"individual":{"type":"array","items":{"$ref":"#/$defs/InputCapture"}}},"required":["container","individual"],"additionalProperties":false}
            SentQuery={"type":"object","properties":{"fields":{"type":"array","items":{"$ref":"#/$defs/SentField"}},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["fields","inputs"],"additionalProperties":false}
            StandardRetryPolicy={"type":"object","properties":{"kind":{"const":"standard"},"attempts":{"type":"integer"},"firstWaitMilliseconds":{"type":"integer"}},"required":["kind","attempts","firstWaitMilliseconds"],"additionalProperties":false}
            SummaryFormDestination={"type":"object","properties":{"kind":{"const":"summary"},"containerId":{"type":"string"}},"required":["kind","containerId"],"additionalProperties":false}
            TargetsBusy={"type":"object","properties":{"kind":{"const":"targets"},"values":{"type":"array","items":{"$ref":"#/$defs/BusyTarget"}}},"required":["kind","values"],"additionalProperties":false}
            TextPathPart={"type":"object","properties":{"kind":{"const":"text"},"text":{"type":"string"}},"required":["kind","text"],"additionalProperties":false}
            TextResponseBody={"type":"object","properties":{"kind":{"const":"text"},"value":{"type":"string"}},"required":["kind","value"],"additionalProperties":false}
            TextScalarWireForm={"type":"object","properties":{"kind":{"const":"text"}},"required":["kind"],"additionalProperties":false}
            TextSentBody={"type":"object","properties":{"kind":{"const":"text"},"value":{"$ref":"#/$defs/ValueRead"},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["kind","value","inputs"],"additionalProperties":false}
            TokenAntiforgery={"type":"object","properties":{"kind":{"const":"token"},"value":{"type":"string"},"place":{"$ref":"#/$defs/TokenPlace"}},"required":["kind","value","place"],"additionalProperties":false}
            TokenPlace={"oneOf":[{"$ref":"#/$defs/HeaderTokenPlace"},{"$ref":"#/$defs/FieldTokenPlace"}]}
            UrlEncodedSentBody={"type":"object","properties":{"kind":{"const":"urlEncoded"},"fields":{"type":"array","items":{"$ref":"#/$defs/SentField"}},"inputs":{"$ref":"#/$defs/SentInputs"}},"required":["kind","fields","inputs"],"additionalProperties":false}
            ValueRead={"oneOf":[{"$ref":"#/$defs/NothingValueRead"},{"$ref":"#/$defs/HeldValueRead"}]}
            WhenNothing={"enum":["leave","empty"]}
            WireForm={"oneOf":[{"$ref":"#/$defs/TextScalarWireForm"},{"$ref":"#/$defs/NumberScalarWireForm"},{"$ref":"#/$defs/BooleanScalarWireForm"},{"$ref":"#/$defs/DateScalarWireForm"},{"$ref":"#/$defs/InstantScalarWireForm"},{"$ref":"#/$defs/WrittenScalarWireForm"},{"$ref":"#/$defs/EnumScalarWireForm"},{"$ref":"#/$defs/FlagsScalarWireForm"},{"$ref":"#/$defs/ListWireForm"}]}
            WrittenScalarWireForm={"type":"object","properties":{"kind":{"const":"written"}},"required":["kind"],"additionalProperties":false}
            """));
    }

    [AcceptedCase]
    public void evaluated_values_and_browser_headers_keep_their_runtime_types()
    {
        var declarations = PlanContract.ReadCore().TypeScript().Split("\n\n");
        Assert.That(declarations.Where(declaration => declaration.StartsWith("export type EvaluatedValue", StringComparison.Ordinal)
                || declaration.StartsWith("export type BrowserHeaders", StringComparison.Ordinal)),
            Is.EqualTo(new[] { "export type EvaluatedValue = unknown;", "export type BrowserHeaders = Headers;" }));
    }

    [AcceptedCase]
    public void a_contract_kind_uses_its_written_property_name_once()
    {
        var contract = PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber,
            new Dictionary<Type, IReadOnlyList<Type>> { [typeof(DayVisit)] = [typeof(NamedResult)] });
        Assert.That(contract.TypeScript().Split("\n\n")[^1], Is.EqualTo(
            """
            export interface DayNamedResult {
              readonly caseKind: "day";
            }

            """));
        Assert.That(JsonSerializer.Serialize<NamedResult>(new DayNamedResult(), Options), Is.EqualTo("{\"caseKind\":\"day\"}"));
    }

    [RefusedCase]
    public void a_contract_kind_with_an_unreadable_property_is_refused()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(info =>
        {
            if (info.Type == typeof(DayNamedResult)) info.Properties[0].Get = null;
        });
        var options = new JsonSerializerOptions(Options) { TypeInfoResolver = resolver };
        Assert.That(Assert.Throws<GrammarRefusedException>(() => PlanContract.Read(options, typeof(Round), ABedIsWrittenAsItsNumber,
            new Dictionary<Type, IReadOnlyList<Type>> { [typeof(DayVisit)] = [typeof(NamedResult)] }))!.Message,
            Is.EqualTo("DayNamedResult is a kind of NamedResult with no text for its kind."));
    }

    [RefusedCase]
    public void two_cases_of_one_contract_union_cannot_write_one_kind()
        => Assert.That(ContractRefusal<RepeatedResult>(),
            Is.EqualTo("DayRepeatedResult and DayRepeatedResult both write the kind 'day'; a kind is one name across RepeatedResult."));

    [AcceptedCase]
    public void one_case_of_a_contract_union_writes_its_kind()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(info =>
        {
            if (info.Type == typeof(RepeatedResult)) info.PolymorphismOptions!.DerivedTypes.RemoveAt(1);
        });
        var options = new JsonSerializerOptions(Options) { TypeInfoResolver = resolver };
        var contract = PlanContract.Read(options, typeof(Round), ABedIsWrittenAsItsNumber,
            new Dictionary<Type, IReadOnlyList<Type>> { [typeof(DayVisit)] = [typeof(RepeatedResult)] });
        Assert.That(string.Join("\n\n", contract.TypeScript().Split("\n\n").TakeLast(2)), Is.EqualTo(
            """
            export type RepeatedResult = DayRepeatedResult;

            export interface DayRepeatedResult {
              readonly kind: "day";
            }

            """));
    }

    [RefusedCase]
    public void a_node_that_is_not_sealed_is_refused()
        => Assert.That(Refusal<OpenRound>(), Is.EqualTo("OpenRound is a node of the plan and is not sealed."));

    [RefusedCase]
    public void a_nullable_field_is_refused()
        => Assert.That(Refusal<RoundWithNoWard>(), Is.EqualTo("RoundWithNoWard.ward is nullable; every field of a node is required and none holds null as a state."));

    [RefusedCase]
    public void a_list_of_nullable_items_is_refused()
        => Assert.That(Refusal<RoundWithGaps>(), Is.EqualTo("RoundWithGaps.nurses is nullable; every field of a node is required and none holds null as a state."));

    [RefusedCase]
    public void a_kind_whose_class_is_not_named_for_it_is_refused()
        => Assert.That(Refusal<RoundWithAMeal>(), Is.EqualTo("Breakfast writes the kind 'breakfast' of Meal; the class of that kind is named BreakfastMeal."));

    [RefusedCase]
    public void a_kind_with_no_text_is_refused()
        => Assert.That(Refusal<RoundWithARota>(), Is.EqualTo("WeekRota is a kind of Rota with no text for its kind."));

    [RefusedCase]
    public void a_field_typed_as_a_kinds_class_is_refused()
        => Assert.That(
            Refusal<RoundOfOneVisit>(),
            Is.EqualTo("RoundOfOneVisit.first is typed as DayVisit, a kind of Visit, so the plan would write it without its kind; type the member as Visit."));

    [RefusedCase]
    public void a_kind_two_unions_write_is_refused()
        => Assert.That(Refusal<RoundWithTwoDays>(), Is.EqualTo("DayHandover and DayVisit both write the kind 'day'; a kind is one name across the plan."));

    [RefusedCase]
    public void a_field_of_a_type_with_no_row_is_refused()
        => Assert.That(Refusal<RoundWithADose>(), Is.EqualTo("RoundWithADose.dose is a System.Decimal; the plan's contract has no row for it."));

    [RefusedCase]
    public void a_choice_that_can_be_made_outside_it_is_refused()
        => Assert.That(
            Refusal<RoundWithAnOpenWing>(),
            Is.EqualTo("OpenWing is a closed choice and a case of it can be made outside it; seal it and make its constructor private."));

    [RefusedCase]
    public void a_choice_that_is_not_sealed_is_refused()
        => Assert.That(
            Refusal<RoundWithALooseWing>(),
            Is.EqualTo("LooseWing is a closed choice and a case of it can be made outside it; seal it and make its constructor private."));

    [RefusedCase]
    public void a_choice_whose_case_is_not_written_as_text_is_refused()
        => Assert.That(Refusal<RoundWithAFloor>(), Is.EqualTo("Floor.Ground is written as 0; a case of a closed choice is written as its text."));

    [RefusedCase]
    public void a_choice_with_two_cases_of_one_text_is_refused()
        => Assert.That(Refusal<RoundWithATwin>(), Is.EqualTo("Twin.Second is written as \"twin\", the text of another case of Twin."));

    [RefusedCase]
    public void two_types_of_one_name_are_refused()
        => Assert.That(
            Refusal<RoundOfTwoWings>(),
            Is.EqualTo("Alis.Reactive.ArchitectureTests.WhenThePlanContractIsGenerated+South+Wing is the second type of the plan named Wing; a name of the contract is one type."));

    [RefusedCase]
    public void a_contract_node_that_is_not_sealed_is_refused()
        => Assert.That(ContractRefusal<OpenRound>(), Is.EqualTo("OpenRound is a node of the plan and is not sealed."));

    [RefusedCase]
    public void a_contract_nullable_field_is_refused()
        => Assert.That(ContractRefusal<RoundWithNoWard>(), Is.EqualTo("RoundWithNoWard.ward is nullable; every field of a node is required and none holds null as a state."));

    [RefusedCase]
    public void a_contract_list_of_nullable_items_is_refused()
        => Assert.That(ContractRefusal<RoundWithGaps>(), Is.EqualTo("RoundWithGaps.nurses is nullable; every field of a node is required and none holds null as a state."));

    [RefusedCase]
    public void a_contract_kind_whose_class_is_not_named_for_it_is_refused()
        => Assert.That(ContractRefusal<RoundWithAMeal>(), Is.EqualTo("Breakfast writes the kind 'breakfast' of Meal; the class of that kind is named BreakfastMeal."));

    [RefusedCase]
    public void a_contract_kind_with_no_text_is_refused()
        => Assert.That(ContractRefusal<RoundWithARota>(), Is.EqualTo("WeekRota is a kind of Rota with no text for its kind."));

    [RefusedCase]
    public void a_contract_field_typed_as_a_kinds_class_is_refused()
        => Assert.That(
            ContractRefusal<RoundOfOneVisit>(),
            Is.EqualTo("RoundOfOneVisit.first is typed as DayVisit, a kind of Visit, so the plan would write it without its kind; type the member as Visit."));

    [RefusedCase]
    public void a_contract_field_of_a_type_with_no_row_is_refused()
        => Assert.That(ContractRefusal<RoundWithADose>(), Is.EqualTo("RoundWithADose.dose is a System.Decimal; the plan's contract has no row for it."));

    [RefusedCase]
    public void a_contract_choice_that_can_be_made_outside_it_is_refused()
        => Assert.That(
            ContractRefusal<RoundWithAnOpenWing>(),
            Is.EqualTo("OpenWing is a closed choice and a case of it can be made outside it; seal it and make its constructor private."));

    [RefusedCase]
    public void a_contract_choice_that_is_not_sealed_is_refused()
        => Assert.That(
            ContractRefusal<RoundWithALooseWing>(),
            Is.EqualTo("LooseWing is a closed choice and a case of it can be made outside it; seal it and make its constructor private."));

    [RefusedCase]
    public void a_contract_choice_whose_case_is_not_written_as_text_is_refused()
        => Assert.That(ContractRefusal<RoundWithAFloor>(), Is.EqualTo("Floor.Ground is written as 0; a case of a closed choice is written as its text."));

    [RefusedCase]
    public void a_contract_choice_with_two_cases_of_one_text_is_refused()
        => Assert.That(ContractRefusal<RoundWithATwin>(), Is.EqualTo("Twin.Second is written as \"twin\", the text of another case of Twin."));

    [RefusedCase]
    public void two_contract_types_of_one_name_are_refused()
        => Assert.That(
            ContractRefusal<RoundOfTwoWings>(),
            Is.EqualTo("Alis.Reactive.ArchitectureTests.WhenThePlanContractIsGenerated+South+Wing is the second type of the plan named Wing; a name of the contract is one type."));

    private static string Refusal<TRoot>()
        => Assert.Throws<GrammarRefusedException>(() => PlanContract.Read(Options, typeof(TRoot), []))!.Message;

    private static string ContractRefusal<TContract>()
        => Assert.Throws<GrammarRefusedException>(() => PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber,
            new Dictionary<Type, IReadOnlyList<Type>> { [typeof(DayVisit)] = [typeof(TContract)] }))!.Message;

    private sealed class VisitReport
    {
        public required Result Result { get; init; }

        public required long Bytes { get; init; }

        public required Shift Shift { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
    [JsonDerivedType(typeof(DayResult))]
    [JsonDerivedType(typeof(NightResult))]
    private abstract class Result
    {
        public abstract string Kind { get; }
    }

    private sealed class DayResult : Result
    {
        public override string Kind => "day";

        public required string Nurse { get; init; }
    }

    private sealed class NightResult : Result
    {
        public override string Kind => "night";
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(CaseKind))]
    [JsonDerivedType(typeof(DayNamedResult))]
    private abstract class NamedResult
    {
        public abstract string CaseKind { get; }
    }

    private sealed class DayNamedResult : NamedResult
    {
        public override string CaseKind => "day";
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
    [JsonDerivedType(typeof(First.DayRepeatedResult))]
    [JsonDerivedType(typeof(Second.DayRepeatedResult))]
    private abstract class RepeatedResult
    {
        public abstract string Kind { get; }
    }

    private static class First
    {
        internal sealed class DayRepeatedResult : RepeatedResult
        {
            public override string Kind => "day";
        }
    }

    private static class Second
    {
        internal sealed class DayRepeatedResult : RepeatedResult
        {
            public override string Kind => "day";
        }
    }

    private interface IWritten
    {
        void Write(Utf8JsonWriter writer);
    }

    private sealed class WrittenConverter<TWritten> : JsonConverter<TWritten>
        where TWritten : IWritten
    {
        public override TWritten Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("A case writes its grammar and never reads it back.");

        public override void Write(Utf8JsonWriter writer, TWritten value, JsonSerializerOptions options) => value.Write(writer);
    }

    internal sealed class Round
    {
        public int Version => 3;

        public required string Ward { get; init; }

        public required Shift Shift { get; init; }

        public required IReadOnlyList<Visit> Visits { get; init; }

        public required IReadOnlyList<Bed> Free { get; init; }

        public required IReadOnlyDictionary<string, bool> Cleaned { get; init; }

        public required JsonElement Notes { get; init; }
    }

    [JsonConverter(typeof(WrittenConverter<Shift>))]
    internal sealed class Shift : IWritten
    {
        internal static readonly Shift Early = new("early");
        internal static readonly Shift Late = new("late");

        private readonly string _text;

        private Shift(string text) => _text = text;

        public void Write(Utf8JsonWriter writer) => writer.WriteStringValue(_text);
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DayVisit), "day")]
    [JsonDerivedType(typeof(NightVisit), "night")]
    internal abstract class Visit;

    internal sealed class DayVisit : Visit
    {
        public required string Nurse { get; init; }

        public required int Beds { get; init; }
    }

    internal sealed class NightVisit : Visit;

    [JsonConverter(typeof(WrittenConverter<Bed>))]
    internal sealed class Bed : IWritten
    {
        public void Write(Utf8JsonWriter writer) => writer.WriteNumberValue(4);
    }

    private class OpenRound
    {
        public required string Ward { get; init; }
    }

    private sealed class RoundWithNoWard
    {
        public string? Ward { get; init; }
    }

    private sealed class RoundWithGaps
    {
        public required IReadOnlyList<string?> Nurses { get; init; }
    }

    private sealed class RoundWithAMeal
    {
        public required Meal Meal { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(Breakfast), "breakfast")]
    private abstract class Meal;

    private sealed class Breakfast : Meal;

    private sealed class RoundWithARota
    {
        public required Rota Rota { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(WeekRota))]
    private abstract class Rota;

    private sealed class WeekRota : Rota;

    private sealed class RoundOfOneVisit
    {
        public required DayVisit First { get; init; }
    }

    private sealed class RoundWithTwoDays
    {
        public required Visit Visit { get; init; }

        public required Handover Handover { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DayHandover), "day")]
    private abstract class Handover;

    private sealed class DayHandover : Handover;

    private sealed class RoundWithADose
    {
        public required decimal Dose { get; init; }
    }

    private sealed class RoundWithAnOpenWing
    {
        public required OpenWing Wing { get; init; }
    }

    [JsonConverter(typeof(WrittenConverter<OpenWing>))]
    private sealed class OpenWing : IWritten
    {
        internal static readonly OpenWing East = new("east");

        private readonly string _text;

        internal OpenWing(string text) => _text = text;

        public void Write(Utf8JsonWriter writer) => writer.WriteStringValue(_text);
    }

    private sealed class RoundWithALooseWing
    {
        public required LooseWing Wing { get; init; }
    }

    [JsonConverter(typeof(WrittenConverter<LooseWing>))]
    private class LooseWing : IWritten
    {
        internal static readonly LooseWing East = new("east");

        private readonly string _text;

        private LooseWing(string text) => _text = text;

        public void Write(Utf8JsonWriter writer) => writer.WriteStringValue(_text);
    }

    private sealed class RoundWithAFloor
    {
        public required Floor Floor { get; init; }
    }

    [JsonConverter(typeof(WrittenConverter<Floor>))]
    private sealed class Floor : IWritten
    {
        internal static readonly Floor Ground = new();

        private Floor()
        {
        }

        public void Write(Utf8JsonWriter writer) => writer.WriteNumberValue(0);
    }

    private sealed class RoundWithATwin
    {
        public required Twin Twin { get; init; }
    }

    [JsonConverter(typeof(WrittenConverter<Twin>))]
    private sealed class Twin : IWritten
    {
        internal static readonly Twin First = new();
        internal static readonly Twin Second = new();

        private Twin()
        {
        }

        public void Write(Utf8JsonWriter writer) => writer.WriteStringValue("twin");
    }

    private sealed class RoundOfTwoWings
    {
        public required North.Wing First { get; init; }

        public required South.Wing Second { get; init; }
    }

    private static class North
    {
        internal sealed class Wing
        {
            public required string Sister { get; init; }
        }
    }

    private static class South
    {
        internal sealed class Wing
        {
            public required string Sister { get; init; }
        }
    }
}
