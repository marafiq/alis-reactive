using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeCheckListBuilder bound to a model property.
    /// Registers in ComponentsMap, creates builder, wires reactive entries, renders.
    ///
    /// Unlike NativeRadioGroup, auto-sync of the hidden input is NOT handled here —
    /// checklist.ts (side-effect TS module) handles syncing all checked checkboxes
    /// into the hidden input's comma-separated value.
    ///
    /// Each checkbox's change event still creates a ComponentEventTrigger entry
    /// so the developer's reactive pipeline fires with the current comma-separated value.
    /// </summary>
    public static class NativeCheckListHtmlExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        public static void NativeCheckList<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeCheckListBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr));

            var builder = new NativeCheckListBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);

            setup.Render(builder);
        }
    }
}
