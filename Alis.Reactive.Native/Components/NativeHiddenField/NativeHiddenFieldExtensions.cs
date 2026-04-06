using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    public static class NativeHiddenFieldExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet("value", ValueProducer.Literal(value));
        }

        // -- Property Write (component source -- cross-plan value binding) --

        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, TypedComponentSource<string> source)
            where TModel : class
        {
            return self.EmitSet("value", source.ToValueProducer());
        }

        // -- Property Write (response body) --

        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<NativeHiddenField, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet("value", ValueProducer.Read(PayloadSource.Success(), "body", Path.Parse(sourcePath)));
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self)
            where TModel : class
        {
            self.Pipeline.Context.EnsureComponent(self.TargetId, _component.Vendor);
            self.Pipeline.Context.EnsureProperty(self.TargetId, _component.ValueMember, _component.ValueMember, Shape.String, "read");
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ValueMember);
        }
    }
}
