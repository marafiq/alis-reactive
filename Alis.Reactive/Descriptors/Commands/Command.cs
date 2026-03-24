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
        /// </summary>
        internal abstract Command WithGuard(Guard guard);
    }
}
