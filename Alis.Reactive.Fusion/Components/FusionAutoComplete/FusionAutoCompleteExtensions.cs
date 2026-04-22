using System;
using System.Linq.Expressions;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionAutoComplete"/>: SetText, SetDataSource,
    /// DataBind, FocusIn/FocusOut, ShowPopup/HidePopup, Enable/Disable.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionAutoCompleteExtensions
    {
        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.EmitSet("text", ValueProducer.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TSource, TProp>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TSource source, Expression<Func<TSource, TProp>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(PayloadSource.Event(), sourcePath, shape: shape));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TResponse, TProp>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, TProp>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            return self.EmitSet("dataSource",
                ValueProducer.Read(source.Scope, sourcePath, shape: shape));
        }

        /// <summary>
        /// Flushes pending property changes to the component in the browser.
        /// Required after <c>SetDataSource</c> in cascade patterns (Changed event).
        /// </summary>
        public static ComponentRef<FusionAutoComplete, TModel> DataBind<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind");

        /// <summary>Moves focus into the autocomplete input.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the autocomplete input.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        /// <summary>Opens the suggestion popup.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("showPopup");

        /// <summary>Closes the suggestion popup.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("hidePopup");

        /// <summary>Enables the autocomplete input for user interaction.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> Enable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet("enabled", ValueProducer.Literal(true));

        /// <summary>Disables the autocomplete input, preventing user interaction.</summary>
        public static ComponentRef<FusionAutoComplete, TModel> Disable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet("enabled", ValueProducer.Literal(false));
    }
}
