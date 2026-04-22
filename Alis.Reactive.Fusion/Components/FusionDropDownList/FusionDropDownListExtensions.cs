using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionDropDownList"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionDropDownListExtensions
    {
        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
            => self.EmitSet("text", ValueProducer.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TSource, TProp>(
            this ComponentRef<FusionDropDownList, TModel> self,
            TSource source, Expression<Func<TSource, TProp>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(PayloadSource.Event(), sourcePath, shape: shape));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TResponse, TProp>(
            this ComponentRef<FusionDropDownList, TModel> self,
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
        public static ComponentRef<FusionDropDownList, TModel> DataBind<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind");

        /// <summary>Moves focus into the dropdown.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the dropdown.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        /// <summary>Opens the dropdown popup.</summary>
        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall("showPopup");

        /// <summary>Closes the dropdown popup.</summary>
        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.EmitCall("hidePopup");
    }
}
