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
        // The declaration's class: its vendor and its input are the class's own, so one class is one component.
        private readonly Type _component;

        private RenderedInput(InputIdentity identity, ComponentVendor vendor, ComponentInput input, Type component)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            ComponentVendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            _component = component ?? throw new ArgumentNullException(nameof(component));
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
            return new RenderedInput(identity, component.Vendor, component.Input, component.GetType());
        }

        internal bool SameAs(RenderedInput other) =>
            _identity.Id.SameAs(other._identity.Id)
            && _identity.Binding.SameAs(other._identity.Binding)
            && _component == other._component
            && Shape == other.Shape;

        internal string Describe() =>
            $"[{ComponentId}, {ComponentVendor.Name}, {Input.Describe()}, {CSharpTypeName.Of(_component)}, {Shape.DescribeContract()}]";
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
