using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Mutates the event args object (ctx.evt) that triggered this reaction.
    /// Supports both set-prop (e.g., e.preventDefaultAction = true) and
    /// call (e.g., e.updateData(data)).
    ///
    /// Same mutation algebra as MutateElementCommand, but the target is
    /// the event args object, not a DOM element. No target ID or vendor needed —
    /// the runtime resolves ctx.evt directly.
    /// </summary>
    public sealed class MutateEventCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "mutate-event";

        public Mutation Mutation { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Value { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? Source { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal MutateEventCommand(Mutation mutation, object? value = null, BindSource? source = null, Guard? when = null)
            : base(when)
        {
            Mutation = mutation;
            Value = value;
            Source = source;
        }

        protected override Command CloneWithGuard(Guard guard)
        {
            return new MutateEventCommand(Mutation, Value, Source, guard);
        }
    }
}
