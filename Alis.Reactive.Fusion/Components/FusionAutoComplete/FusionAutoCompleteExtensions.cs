using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive Plan pipeline extensions for reading from and updating <see cref="FusionAutoComplete"/>.
    /// </summary>
    public static class FusionAutoCompleteExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentProperty<string> TextProperty =
            ComponentProperty<string>.Named("text");

        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentProperty<bool> EnabledProperty =
            ComponentProperty<bool>.Named("enabled");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        /// <summary>Sets the selected value.</summary>
        /// <param name="value">The value to select, or <see langword="null"/> to clear the selection.</param>
        public static ComponentRef<FusionAutoComplete, TModel> SetValue<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueExpression.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
        }

        /// <summary>
        /// Replaces the data source with a typed array source, including client-side
        /// <see cref="Alis.Reactive.Builders.Arrays.ReactiveArray{T}"/> values from <c>AsSource()</c>.
        /// </summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>
        /// Applies pending AutoComplete property changes to the rendered component.
        /// </summary>
        /// <remarks>
        /// Required after <c>SetDataSource</c> in cascade patterns (Changed event).
        /// Not needed when using <c>updateData()</c> in filtering patterns.
        /// </remarks>
        public static ComponentRef<FusionAutoComplete, TModel> DataBind<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Moves focus into the autocomplete input.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the autocomplete input.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Opens the suggestion popup.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the suggestion popup.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        // Syncfusion AutoComplete spinner methods have no visible effect here,
        // and refresh() steals focus during filtering. Omitted intentionally.

        /// <summary>Enables the autocomplete input for user interaction.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> Enable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet(EnabledProperty, ValueExpression.Literal(true));

        /// <summary>Disables the autocomplete input, preventing user interaction.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> Disable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet(EnabledProperty, ValueExpression.Literal(false));

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
