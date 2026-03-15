using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Descriptors.Requests
{
    public sealed class StatusHandler
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? Commands { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Reaction? Reaction { get; }

        /// <summary>Default handler — sequential commands only.</summary>
        public StatusHandler(List<Command> commands)
        {
            StatusCode = null;
            Commands = commands;
        }

        /// <summary>Status-specific handler — sequential commands only.</summary>
        public StatusHandler(int statusCode, List<Command> commands)
        {
            StatusCode = statusCode;
            Commands = commands;
        }

        /// <summary>Default handler — full reaction (conditional, http, etc.).</summary>
        public StatusHandler(Reaction reaction)
        {
            StatusCode = null;
            Reaction = reaction;
        }

        /// <summary>Status-specific handler — full reaction.</summary>
        public StatusHandler(int statusCode, Reaction reaction)
        {
            StatusCode = statusCode;
            Reaction = reaction;
        }
    }
}
