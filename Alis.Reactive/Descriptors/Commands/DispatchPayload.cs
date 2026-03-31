using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Values;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Object-shaped payload carried by a dispatched custom event.
    /// </summary>
    /// <remarks>
    /// Each payload field is described by the same <see cref="CommandValue"/> contract used by
    /// property writes and method arguments. The runtime resolves these field descriptors and
    /// emits the final <c>CustomEvent.detail</c> object at dispatch time.
    /// </remarks>
    [JsonConverter(typeof(DispatchPayloadJsonConverter))]
    public sealed class DispatchPayload
    {
        private static readonly JsonSerializerOptions ProjectionOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IReadOnlyDictionary<string, CommandValue> _fields;

        /// <summary>
        /// Gets the payload field descriptors keyed by field name.
        /// </summary>
        public IReadOnlyDictionary<string, CommandValue> Fields => _fields;

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal DispatchPayload(IDictionary<string, CommandValue> fields)
        {
            _fields = new ReadOnlyDictionary<string, CommandValue>(
                new Dictionary<string, CommandValue>(fields, StringComparer.Ordinal));
        }

        internal static DispatchPayload FromObject<TPayload>(TPayload payload)
        {
            if (payload is DispatchPayload dispatchPayload)
            {
                return dispatchPayload;
            }

            var json = JsonSerializer.SerializeToElement(payload, ProjectionOptions);
            if (json.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    "Dispatch payloads must serialize to a JSON object so each field can flow through the plan value contract.");
            }

            var fields = new Dictionary<string, CommandValue>(StringComparer.Ordinal);
            foreach (var property in json.EnumerateObject())
            {
                fields[property.Name] = CommandValue.FromLiteral(property.Value.Clone());
            }

            return new DispatchPayload(fields);
        }
    }
}
