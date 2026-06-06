using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive Plan pipeline extensions for reading from and updating <see cref="FusionDropDownList"/>.
    /// </summary>
    public static class FusionDropDownListExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

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
        /// <param name="value">Selected value, or <see langword="null"/> to clear.</param>
        public static ComponentRef<FusionDropDownList, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueExpression.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionDropDownList, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionDropDownList, TModel> self,
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
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionDropDownList, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>Applies pending DropDownList property changes to the rendered component.</summary>
        /// <remarks>
        /// Call after <see cref="SetDataSource{TModel,TSource}"/> or
        /// <see cref="SetDataSource{TModel,TResponse}"/> to make the new items appear.
        /// </remarks>
        public static ComponentRef<FusionDropDownList, TModel> DataBind<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Moves focus into the dropdown.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the dropdown.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Opens the dropdown popup.</summary>
        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the dropdown popup.</summary>
        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Reads the selected value for conditions or gather.</summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
