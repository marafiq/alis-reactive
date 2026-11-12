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

    [AcceptedCase]
    public void a_closed_grammar_is_written_as_its_typescript()
        => Assert.That(PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber).TypeScript(), Is.EqualTo(
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

            """));

    [AcceptedCase]
    public void a_closed_grammar_is_written_as_its_schema()
        => Assert.That(PlanContract.Read(Options, typeof(Round), ABedIsWrittenAsItsNumber).Schema(), Is.EqualTo(
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
                }
              }
            }

            """));

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

    private static string Refusal<TRoot>()
        => Assert.Throws<GrammarRefusedException>(() => PlanContract.Read(Options, typeof(TRoot), []))!.Message;

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
