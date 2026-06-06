using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for reading from and updating <see cref="FusionMultiColumnComboBox"/>.
    /// </summary>
    public static class FusionMultiColumnComboBoxExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentProperty<string> TextProperty =
            ComponentProperty<string>.Named("text");

        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

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
        /// <param name="value">The value to select, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueExpression.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
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
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>Flushes pending property changes through the Syncfusion MultiColumnComboBox instance.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Moves focus into the combo box.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the combo box.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Opens the multi-column dropdown popup.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the multi-column dropdown popup.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the combo box's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
