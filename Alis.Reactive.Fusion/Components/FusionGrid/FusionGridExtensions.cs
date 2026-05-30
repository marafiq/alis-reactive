using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionGrid"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionGrid&gt;("residents-grid").SetDataSource(json, j =&gt; j.Result)</c>.
    /// </para>
    /// <para>
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// </para>
    /// </remarks>
    public static class FusionGridExtensions
    {
        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentMethod RefreshMethod =
            ComponentMethod.Named("refresh");

        /// <summary>
        /// Replaces the grid data source with items from an HTTP response body.
        /// Typically used in custom binding to push server-filtered/sorted data.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="self">The grid component reference.</param>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
        }

        /// <summary>
        /// Replaces the grid data source with the entire HTTP response body.
        /// Use for SF Grid custom binding where the response shape is
        /// <c>{ result: [...], count: N }</c>.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="self">The grid component reference.</param>
        /// <param name="source">The response body instance.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
            => self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, "responseBody"));

        /// <summary>
        /// Replaces the grid data source with items from an event payload.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="self">The grid component reference.</param>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionGrid, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueExpression.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>
        /// Replaces the grid data source with a typed array source — including a client-side
        /// <see cref="Alis.Reactive.Builders.Arrays.ReactiveArray{T}"/> transform exposed via
        /// <c>AsSource()</c>. Routes any array value (component member, plugin, event payload,
        /// or a filtered/sorted array) into the grid's data source with no HTTP round-trip.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TElement">The row element type carried by the source.</typeparam>
        /// <param name="self">The grid component reference.</param>
        /// <param name="source">The typed array source.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionGrid, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>
        /// Reads the grid's current rows as a typed array source — the read counterpart of the
        /// typed-array <c>SetDataSource</c> overload. Feed it to <c>p.From(...)</c> to
        /// filter/sort/aggregate the rows already on screen, then rebind, without re-fetching
        /// from the server.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TRow">The row element type.</typeparam>
        /// <param name="self">The grid component reference.</param>
        /// <returns>A typed source over the grid's current data.</returns>
        public static TypedComponentSource<TRow[]> Data<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            where TRow : class
            => self.Read(ComponentProperty<TRow[]>.Named("dataSource"));

        /// <summary>
        /// Triggers a grid refresh. Call after <see cref="SetDataSource{TModel, TResponse}"/>
        /// to re-render with the new data.
        /// </summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionGrid, TModel> Refresh<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshMethod);
    }
}
