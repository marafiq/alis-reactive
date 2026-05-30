using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentMethod RefreshMethod =
            ComponentMethod.Named("refresh");

        /// <summary>
        /// Replaces the grid data source with items from an HTTP response body.
        /// Typically used in custom binding to push server-filtered/sorted data.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse, TValue>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, TValue>> path)
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
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
            => self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, "responseBody"));

        /// <summary>
        /// Replaces the grid data source with items from an event payload.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TSource, TValue>(
            this ComponentRef<FusionGrid, TModel> self,
            TSource source, Expression<Func<TSource, TValue>> path)
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
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionGrid, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
            => self.EmitSet(DataSourceProperty, source.ToValueExpression());

        /// <summary>
        /// Reads the grid's current rows as a typed array source — the read counterpart of the
        /// typed-array <see cref="SetDataSource{TModel, TElement}(ComponentRef{FusionGrid, TModel}, TypedSource{TElement[]})"/>
        /// overload. Feed it to <c>p.From(...)</c> to filter/sort/aggregate the rows already on
        /// screen, then rebind, without re-fetching from the server.
        /// </summary>
        public static TypedComponentSource<TRow[]> Data<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            where TRow : class
            => self.Read(ComponentProperty<TRow[]>.Named("dataSource"));

        /// <summary>
        /// Triggers a grid refresh through Syncfusion's public refresh method.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> Refresh<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshMethod);
    }
}
