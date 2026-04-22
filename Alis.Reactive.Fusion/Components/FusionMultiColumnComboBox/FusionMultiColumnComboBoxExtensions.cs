using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionMultiColumnComboBox"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionMultiColumnComboBoxExtensions
    {
        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string text)
            where TModel : class
            => self.EmitSet("text", ValueProducer.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TSource, TProp>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            TSource source, Expression<Func<TSource, TProp>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(PayloadSource.Event(), sourcePath, shape: shape));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TResponse, TProp>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
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
        public static ComponentRef<FusionMultiColumnComboBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind");

        /// <summary>Moves focus into the combo box.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the combo box.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        /// <summary>Opens the multi-column dropdown popup.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall("showPopup");

        /// <summary>Closes the multi-column dropdown popup.</summary>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall("hidePopup");
    }
}
