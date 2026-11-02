using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Grammar;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Inputs
{
    /// <summary>
    /// An input a page rendered through <c>Html.InputField</c>: its identity, and what its component declares
    /// as its input, the path of its value and the event that commits it. The plan carries it so that
    /// validation reads the value where the component holds it and the form re-checks the field on the event
    /// the component commits with.
    /// </summary>
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

        /// <summary>The path from the component's object to its value; the model reads the value in the model's type.</summary>
        internal Path ValuePath { get; }

        /// <summary>The event the component raises when the person commits a value; the form re-checks the field on it.</summary>
        internal EventName CommittedBy { get; }

        internal Shape Shape => _identity.ValueShape;
    }

    /// <summary>The inputs one plan rendered, by binding path.</summary>
    internal sealed class RenderedInputs
    {
        private readonly Dictionary<string, RenderedInput> _inputs = new Dictionary<string, RenderedInput>(StringComparer.Ordinal);

        internal IReadOnlyList<KeyValuePair<string, RenderedInput>> Entries =>
            new List<KeyValuePair<string, RenderedInput>>(_inputs);

        // A field joins its plan once: the page refuses an element id rendered a second time before the input joins
        // (RenderedPage), and one plan's binding path names one element id.
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
