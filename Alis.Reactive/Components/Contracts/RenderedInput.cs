using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Grammar;
using Alis.Reactive.InputField;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// An input a page rendered through <c>Html.InputField</c>: its identity, and what its component
    /// declares. The plan carries it so that validation reads the value where the component holds it
    /// and the form re-checks the field on the event the component commits with.
    /// </summary>
    internal sealed class RenderedInput
    {
        private readonly InputIdentity _identity;

        private RenderedInput(InputIdentity identity, ComponentVendor vendor, ComponentInput input)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            ComponentVendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            Input = input ?? throw new ArgumentNullException(nameof(input));
        }

        internal string ComponentId => _identity.ElementId;

        internal ComponentVendor ComponentVendor { get; }

        internal string BindingPath => _identity.BindingName;

        internal BindingPath Binding => _identity.Binding;

        internal ComponentInput Input { get; }

        internal Shape Shape => _identity.ValueShape;

        internal static RenderedInput Of(InputIdentity identity, IInputComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return new RenderedInput(identity, component.Vendor, component.Input);
        }
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
