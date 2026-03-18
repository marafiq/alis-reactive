using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Native.Components
{
    public static class NativeRadioGroupExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<NativeRadioGroup, TModel> SetValue<TModel, TSource>(
            this ComponentRef<NativeRadioGroup, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("value"), source: new EventSource(sourcePath));
        }

        public static ComponentRef<NativeRadioGroup, TModel> FocusIn<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
