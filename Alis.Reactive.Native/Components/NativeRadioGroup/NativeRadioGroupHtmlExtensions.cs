using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeRadioGroupBuilder bound to a model property.
    /// Registers in ComponentsMap, creates builder, renders.
    ///
    /// Auto-sync (radio value → hidden input) is handled by radio-group.ts,
    /// which discovers containers via [data-reactive-radio-group] and wires
    /// change events. This keeps the plan clean — no N auto-sync entries.
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
            setup.Render(builder);
        }
    }
}
