using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Base class for request input strategies. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]
    public abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no authored input.</summary>
    public sealed class NoRequestInput : RequestInput
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";
    }

}
