using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class AllGuard : Guard
    {
        [System.Text.Json.Serialization.JsonPropertyOrder(-1)]
        public string Kind => "all";

        public IReadOnlyList<Guard> Guards { get; }

        public AllGuard(IReadOnlyList<Guard> guards)
        {
            if (guards == null || guards.Count == 0)
                throw new ArgumentException("AllGuard requires at least one guard.", nameof(guards));
            Guards = guards;
        }
    }
}
