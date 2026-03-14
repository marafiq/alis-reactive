using System;
using System.Linq.Expressions;
using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for TestWidgetSyncFusion.
    ///
    /// Maps 1:1 to the JS TestWidget API via jsEmit:
    ///   Property write → el.value=val
    ///   Property read  → ReadProperty
    ///   Void method    → el.focus(), el.clear()
    ///   Method + arg   → el.setItems(val)
    ///
    /// Source overloads follow the same pattern as ElementBuilder.SetText:
    ///   (TSource, Expression)       → ExpressionPathHelper.ToEventPath → EventSource
    ///   (ResponseBody, Expression)  → ExpressionPathHelper.ToResponsePath → EventSource
    ///   (TypedSource)               → TypedSource.ToBindSource → ComponentSource
    ///
    /// All use existing building blocks. Zero DSL changes.
    /// </summary>
    public static class TestWidgetSyncFusionExtensions
    {
        // ── Property Write (static) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, string value)
            where TModel : class => self.Emit("el.value=val", value);

        // ── Property Write (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit("el.value=val", source: new EventSource(sourcePath));
        }

        // ── Property Write (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit("el.value=val", source: new EventSource(sourcePath));
        }

        // ── Property Write (component read) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TProp>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, TypedSource<TProp> source)
            where TModel : class
            => self.Emit("el.value=val", source: source.ToBindSource());

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
            where TModel : class => self.Emit("el.focus()");

        public static ComponentRef<TestWidgetSyncFusion, TModel> Clear<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit("el.clear()");

        // ── Method + arg (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit("el.setItems(val)", source: new EventSource(sourcePath));
        }

        // ── Method + arg (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit("el.setItems(val)", source: new EventSource(sourcePath));
        }
    }
}
