using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionInPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionInPlaceEditor inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.DateOfBirth)</c>, then call
    /// <c>.FusionInPlaceEditor(b =&gt; { b.Type(InputType.Date).Mode(RenderMode.Inline); })</c>.
    /// Use the vendor builder directly for configuration (<c>Type</c>, <c>Mode</c>, <c>EditableOn</c>,
    /// <c>ShowButtons</c>, etc.) and <c>.Reactive(plan, evt =&gt; evt.ActionBegin, …)</c> to own the commit flow.
    /// </remarks>
    public static class FusionInPlaceEditorHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionInPlaceEditor bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TProp">The model value type rendered by the in-place editor.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to configure the editor (type, mode, inner model, reactive events).</param>
        public static void FusionInPlaceEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<InPlaceEditorBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var initialValue = InitialEditorValue.FromModel(
                setup.Expression,
                setup.Helper.ViewData.Model);

            var builder = setup.Helper.EJS().InPlaceEditor(setup.ElementId)
                .Name(setup.BindingPath)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId });

            builder = initialValue.ApplyTo(builder);

            build(builder);
            setup.Render(builder.Render());
        }
    }

    internal abstract class InitialEditorValue
    {
        private protected InitialEditorValue() { }

        internal static InitialEditorValue FromModel<TModel, TProp>(
            System.Linq.Expressions.Expression<Func<TModel, TProp>> expression,
            TModel model)
            where TModel : class
        {
            try
            {
                var compiled = expression.Compile();
                var value = compiled(model);
                return value == null
                    ? Missing
                    : new PresentInitialEditorValue(value);
            }
            catch
            {
                return Missing;
            }
        }

        private static InitialEditorValue Missing { get; } =
            new MissingInitialEditorValue();

        internal abstract InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder);
    }

    internal sealed class MissingInitialEditorValue : InitialEditorValue
    {
        internal override InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder) => builder;
    }

    internal sealed class PresentInitialEditorValue : InitialEditorValue
    {
        private readonly object _value;

        internal PresentInitialEditorValue(object value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override InPlaceEditorBuilder ApplyTo(InPlaceEditorBuilder builder) =>
            builder.Value(_value);
    }
}
