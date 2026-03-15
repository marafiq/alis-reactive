using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Injects the HTTP response body as innerHTML of the target element.
    /// Used for loading partial views via GET requests.
    /// </summary>
    public sealed class IntoCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "into";

        public string Target { get; }

        public IntoCommand(string target)
        {
            Target = target;
        }
    }
}
