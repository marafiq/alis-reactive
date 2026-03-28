using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class Branch
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? Guard { get; }
        public Reaction Reaction { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal Branch(Guard? guard, Reaction reaction)
        {
            Guard = guard;
            Reaction = reaction;
        }
    }
}
