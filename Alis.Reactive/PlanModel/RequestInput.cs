using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Base class for request body strategies. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]
    public abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no body or gathered input.</summary>
    public sealed class NoRequestInput : RequestInput
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";
    }

    /// <summary>Sends a single evaluated value as the request body.</summary>
    public sealed class ValueInput : RequestInput
    {
        private readonly RequestTransport _transport;

        /// <summary>Gets the kind. Always <c>"value"</c>.</summary>
        public string Kind => "value";
        /// <summary>Gets the value expression to send as the body.</summary>
        public ObjectProducer Value { get; }
        /// <summary>Gets the transport format (json or form).</summary>
        public string Transport => _transport.Value;

        internal ValueInput(ObjectProducer value, RequestTransport transport)
        {
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
            _transport = transport ?? throw new System.ArgumentNullException(nameof(transport));
        }
    }
}
