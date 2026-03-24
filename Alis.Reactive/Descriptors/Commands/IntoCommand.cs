using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

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

        public IntoCommand(string target, Guard? when = null)
            : base(when)
        {
            Target = target;
        }

        protected override Command CloneWithGuard(Guard guard)
        {
            return new IntoCommand(Target, guard);
        }
    }
}
