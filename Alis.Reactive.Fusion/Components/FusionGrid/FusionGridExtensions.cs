using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionGrid — SetDataSource, Refresh.
    /// Non-input component: no Value() read, no SetValue.
    /// </summary>
    public static class FusionGridExtensions
    {
        /// <summary>
        /// Sets the grid's dataSource from an HTTP response sub-path: ej2.dataSource = resolved.
        /// Use for plain array data (client-side paging).
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>
        /// Sets the grid's dataSource to the entire HTTP response body: ej2.dataSource = responseBody.
        /// Use for server-side paging in custom binding mode — response must be {result: [...], count: N}.
        /// SF Grid reads result for rows and count for pager totalRecordsCount.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
            => self.Emit(new SetPropMutation("dataSource"), source: new EventSource("responseBody"));

        /// <summary>
        /// Sets the grid's dataSource from an event payload: ej2.dataSource = resolved.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionGrid, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>
        /// Refreshes the grid: ej2.refresh().
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> Refresh<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("refresh"));
    }
}
