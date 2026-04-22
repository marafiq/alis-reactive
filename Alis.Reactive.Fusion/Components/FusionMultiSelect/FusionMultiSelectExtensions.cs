using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionMultiSelect"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionMultiSelectExtensions
    {
        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TSource, TProp>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            TSource source, Expression<Func<TSource, TProp>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(PayloadSource.Event(), sourcePath, shape: shape));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TResponse, TProp>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, TProp>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(source.Scope, sourcePath, shape: shape));
        }

        /// <summary>Flushes pending property changes to the component in the browser.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind");

        /// <summary>Opens the selection popup.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall("showPopup");

        /// <summary>Closes the selection popup.</summary>
        public static ComponentRef<FusionMultiSelect, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.EmitCall("hidePopup");
    }
}
