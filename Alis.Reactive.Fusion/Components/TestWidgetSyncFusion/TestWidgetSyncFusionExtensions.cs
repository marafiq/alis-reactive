using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Values;

namespace Alis.Reactive.Fusion.Components
{
    public static class TestWidgetSyncFusionExtensions
    {
        private static readonly TestWidgetSyncFusion _component = new TestWidgetSyncFusion();

        // ── Property Write (static) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, string value)
            where TModel : class => self.Emit(new SetPropMutation("value", CommandValue.FromLiteral(value)));

        // ── Property Write (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("value", CommandValue.FromSource(new EventSource(sourcePath))));
        }

        // ── Property Write (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("value", CommandValue.FromSource(new EventSource(sourcePath))));
        }

        // ── Property Write (component read) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TProp>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, TypedSource<TProp> source)
            where TModel : class
            => self.Emit(new SetPropMutation("value", CommandValue.FromSource(source.ToBindSource())));

        // ── Property Read ──

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);

        // ── Void Method (no args) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit(new CallMutation("focus"));

        public static ComponentRef<TestWidgetSyncFusion, TModel> Clear<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit(new CallMutation("clear"));

        // ── Method + arg (event payload) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new CallMutation("setItems", args: new[] { CommandValue.FromSource(new EventSource(sourcePath)) }));
        }

        // ── Method + arg (response body) ──

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new CallMutation("setItems", args: new[] { CommandValue.FromSource(new EventSource(sourcePath)) }));
        }
    }
}
