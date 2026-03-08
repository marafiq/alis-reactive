using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class Branch
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? Guard { get; }
        public Reaction Reaction { get; }

        public Branch(Guard? guard, Reaction reaction)
        {
            Guard = guard;
            Reaction = reaction;
        }
    }
}
