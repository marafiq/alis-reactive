using System;
using System.Linq.Expressions;
using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for TestWidgetSyncFusion.
    ///
    /// Maps 1:1 to the JS TestWidget API via structured prop/method fields:
    ///   Property write → { prop: "value" }
    ///   Property read  → ReadProperty
    ///   Void method    → { method: "focus" }, { method: "clear" }
    ///   Method + arg   → { method: "setItems" }
    ///
    /// Source overloads follow the same pattern as ElementBuilder.SetText:
    ///   (TSource, Expression)       → ExpressionPathHelper.ToEventPath → EventSource
    ///   (ResponseBody, Expression)  → ExpressionPathHelper.ToResponsePath → EventSource
    ///   (TypedSource)               → TypedSource.ToBindSource → ComponentSource
    /// </summary>
    public static class TestWidgetSyncFusionExtensions
    {
        // ── Property Write (static) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, string value)
            where TModel : class => self.Emit(prop: "value", value: value);

        // ── Property Write (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(prop: "value", source: new EventSource(sourcePath));
        }

        // ── Property Write (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(prop: "value", source: new EventSource(sourcePath));
        }

        // ── Property Write (component read) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TProp>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, TypedSource<TProp> source)
            where TModel : class
            => self.Emit(prop: "value", source: source.ToBindSource());

        // ── Property Read ──

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.ReadProperty<string>("value");

        public static TypedComponentSource<bool> Focused<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.ReadProperty<bool>("focused");

        // ── Void Method (no args) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit(method: "focus");

        public static ComponentRef<TestWidgetSyncFusion, TModel> Clear<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit(method: "clear");

        // ── Method + arg (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(method: "setItems", source: new EventSource(sourcePath));
        }

        // ── Method + arg (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(method: "setItems", source: new EventSource(sourcePath));
        }
    }
}
