using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(ValidationConditionJsonConverter))]
    internal sealed class ValidationCondition
    {
        private readonly Condition _predicate;

        private ValidationCondition(Condition condition)
        {
            _predicate = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, _predicate, options);

        internal static ValidationCondition FromDeterministicCondition(Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (ContainsConfirmationPrompt(condition))
                throw new ArgumentException(
                    "Validation activation conditions must be deterministic. Move Confirm(...) to a reactive branch before validation runs.",
                    nameof(condition));

            return new ValidationCondition(condition);
        }

        private static bool ContainsConfirmationPrompt(Condition condition)
        {
            return condition switch
            {
                CompareCondition => false,
                AllCondition all => all.Terms.Any(ContainsConfirmationPrompt),
                AnyCondition any => any.Terms.Any(ContainsConfirmationPrompt),
                NotCondition not => ContainsConfirmationPrompt(not.Term),
                ConfirmCondition => true,
                _ => throw new InvalidOperationException(
                    "Validation condition cannot inspect unknown condition type '" + condition.GetType().FullName + "'.")
            };
        }
    }

    internal sealed class ValidationConditionJsonConverter : JsonConverter<ValidationCondition>
    {
        public override void Write(Utf8JsonWriter writer, ValidationCondition value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            value.WriteTo(writer, options);
        }

        public override ValidationCondition Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }
}
