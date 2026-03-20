using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionAutoComplete (SetValue, SetDataSource, FocusIn, ShowPopup, etc.).
    /// </summary>
    public static class FusionAutoCompleteExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        public static ComponentRef<FusionAutoComplete, TModel> SetValue<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        // ── SetDataSource (event payload) ──

        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        // ── SetDataSource (response body) ──

        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>
        /// Flushes pending property changes to the SF instance.
        /// Required after SetDataSource in cascade patterns (Changed event).
        /// NOT needed with updateData() in filtering patterns.
        /// </summary>
        public static ComponentRef<FusionAutoComplete, TModel> DataBind<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"));

        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        // NOTE: showSpinner/hideSpinner have no visible effect on SF AutoComplete.
        // refresh() causes focus loss mid-typing — not usable during filtering.
        // Both verified manually. Omitted intentionally.

        public static ComponentRef<FusionAutoComplete, TModel> Enable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("enabled"), value: true);

        public static ComponentRef<FusionAutoComplete, TModel> Disable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("enabled"), value: false);

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
