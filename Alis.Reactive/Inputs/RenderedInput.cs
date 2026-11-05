using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Grammar;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Inputs
{
    internal sealed class RenderedInput
    {
        private readonly InputIdentity _identity;

        internal RenderedInput(InputIdentity identity, ComponentVendor vendor, Path valuePath, EventName committedBy)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            ComponentVendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            ValuePath = valuePath ?? throw new ArgumentNullException(nameof(valuePath));
            CommittedBy = committedBy ?? throw new ArgumentNullException(nameof(committedBy));
        }

        internal string ComponentId => _identity.ElementId;

        internal ComponentVendor ComponentVendor { get; }

        internal string BindingPath => _identity.BindingName;

        internal BindingPath Binding => _identity.Binding;

        internal Path ValuePath { get; }

        internal EventName CommittedBy { get; }

        internal Shape Shape => _identity.ValueShape;
    }

    internal sealed class RenderedInputs
    {
        private readonly Dictionary<string, RenderedInput> _inputs = new Dictionary<string, RenderedInput>(StringComparer.Ordinal);

        internal IReadOnlyList<KeyValuePair<string, RenderedInput>> Entries
            => new List<KeyValuePair<string, RenderedInput>>(_inputs);

        internal void Add(RenderedInput input) => _inputs.Add(input.BindingPath, input);

        internal bool TryFindForComponent(ComponentId componentId, [NotNullWhen(true)] out RenderedInput? input)
        {
            foreach (var candidate in _inputs.Values)
            {
                if (candidate.ComponentId == componentId.Value)
                {
                    input = candidate;
                    return true;
                }
            }

            input = null;
            return false;
        }
    }
}
