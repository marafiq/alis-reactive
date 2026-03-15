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
        public Guard? When { get; private set; }

        internal void GuardWith(Guard guard)
        {
            if (!(When is null))
                throw new InvalidOperationException(
                    "Command already has a guard. Each command can only have one When guard.");
            When = guard;
        }
    }
}
