using System;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Command>))]
    public abstract class Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? When { get; }

        protected Command(Guard? when = null)
        {
            When = when;
        }

        /// <summary>
        /// Returns a new Command of the same kind with the guard attached.
        /// Immutable — the original instance is unchanged.
        /// Throws if this command already has a guard (A-T1).
        /// </summary>
        internal Command WithGuard(Guard guard)
        {
            if (When != null)
                throw new InvalidOperationException(
                    "Command already has a guard. Each command can only have one When guard.");
            return CloneWithGuard(guard);
        }

        /// <summary>Subclass hook — creates a new instance of the same kind with the guard set.</summary>
        protected abstract Command CloneWithGuard(Guard guard);
    }
}
