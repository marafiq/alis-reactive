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
        private readonly ComponentKind _kind;

        private RenderedInput(InputIdentity identity, ComponentVendor vendor, ComponentInput input, ComponentKind kind)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            ComponentVendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
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
            return new RenderedInput(identity, component.Vendor, component.Input, ComponentKind.Of(CSharpTypeName.Of(component.GetType())));
        }

        internal bool SameAs(RenderedInput other) =>
            _identity.Id.SameAs(other._identity.Id)
            && ComponentVendor.Equals(other.ComponentVendor)
            && _identity.Binding.SameAs(other._identity.Binding)
            && Input.Matches(other.Input)
            && _kind.SameAs(other._kind)
            && Shape == other.Shape;

        internal string Describe() =>
            $"[{ComponentId}, {ComponentVendor.Name}, {Input.Describe()}, {_kind.Value}, {Shape.DescribeContract()}]";
    }

    /// <summary>The inputs one plan rendered, by binding path. One field is rendered once.</summary>
    internal sealed class RenderedInputs
    {
        private readonly Dictionary<string, RenderedInput> _inputs = new Dictionary<string, RenderedInput>(StringComparer.Ordinal);

        internal IReadOnlyDictionary<string, RenderedInput> Snapshot() =>
            new Dictionary<string, RenderedInput>(_inputs, StringComparer.Ordinal);

        internal IReadOnlyList<KeyValuePair<string, RenderedInput>> Entries =>
            new List<KeyValuePair<string, RenderedInput>>(_inputs);

        // A partial rendered twice adds the same input twice; a different component on the same field is
        // a defect of the view.
        internal void Add(RenderedInput input)
        {
            if (_inputs.TryGetValue(input.BindingPath, out var existing))
            {
                if (existing.SameAs(input)) return;
                throw new InvalidOperationException(
                    $"Input '{input.BindingPath}' is rendered twice with different components: " +
                    $"first {existing.Describe()}, then {input.Describe()}.");
            }

            _inputs[input.BindingPath] = input;
        }

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
