using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Wire base for request input strategies authored through gather builders.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<RequestInput>))]
    internal abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no authored input.</summary>
    internal sealed class NoRequestInput : RequestInput
    {
        /// <summary>JSON discriminator for bodiless request input. Always <c>"none"</c>.</summary>
        public string Kind => "none";
    }

}
