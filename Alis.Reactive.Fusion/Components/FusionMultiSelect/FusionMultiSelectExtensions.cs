using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for reading and mutating <see cref="FusionMultiSelect"/>.
    /// </summary>
    /// <remarks>
    /// Use these from a <see cref="ComponentRef{TComponent, TModel}"/> resolved by the pipeline:
    /// <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Skills).SetValue(new[] { "C#", "SQL" })</c>.
    /// </remarks>
    public static class FusionMultiSelectExtensions
    {
        private static readonly FusionMultiSelect Component = new FusionMultiSelect();

        private static readonly ComponentProperty<string[]> ValueProperty =
            ComponentProperty<string[]>.Named(Component.ValueMember);

        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        /// <summary>Replaces the selected value array.</summary>
        /// <param name="value">The values to select, or <see langword="null"/> to clear the selection.</param>
        public static ComponentRef<FusionMultiSelect, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self, string[]? value)
            where TModel : class
            => self.EmitSet(ValueProperty, value == null
                ? ValueExpression.Null()
                : ValueExpression.LiteralRaw(value, Shape.ArrayOf(Shape.String)));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="source">Provides the event payload type; runtime reads the active event payload.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="source">The response scope used by the generated value expression.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
        }

        /// <summary>
        /// Replaces the data source with a typed array source — including a client-side
        /// <see cref="Alis.Reactive.Builders.Arrays.ReactiveArray{T}"/> transform via <c>AsSource()</c>.
        /// Routes any array value into the option list with no HTTP round-trip.
        /// </summary>
        /// <param name="source">The typed array source.</param>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>Flushes pending property changes through the Syncfusion MultiSelect instance.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Opens the selection popup.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the selection popup.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Reads the current selected value array for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Skills).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the multi-select's current values.</returns>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
