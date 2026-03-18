using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeRadioGroupBuilder bound to a model property.
    /// Registers in ComponentsMap, creates builder, wires auto-sync entries, renders.
    ///
    /// Auto-sync: each radio's change event copies its value to the hidden input.
    /// This is inherent to how radio groups work — N elements, 1 logical value.
    /// The hidden input is the canonical element for evalRead/gather/validation.
    /// </summary>
    public static class NativeRadioGroupHtmlExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        public static void NativeRadioGroup<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeRadioGroupBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr));

            var builder = new NativeRadioGroupBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);

            // Auto-sync: each radio's change event → hidden input .value
            for (int i = 0; i < builder.Options.Count; i++)
            {
                var pb = new PipelineBuilder<TModel>();
                pb.AddCommand(new MutateElementCommand(
                    builder.ElementId,
                    new SetPropMutation("value"),
                    source: new EventSource("evt.value"),
                    vendor: _component.Vendor));

                var radioId = $"{builder.ElementId}_r{i}";
                var trigger = new ComponentEventTrigger(
                    radioId, "change", _component.Vendor,
                    builder.BindingPath, _component.ReadExpr);

                setup.Plan.AddEntry(new Entry(trigger, pb.BuildReaction()));
            }

            setup.Render(builder);
        }
    }
}
