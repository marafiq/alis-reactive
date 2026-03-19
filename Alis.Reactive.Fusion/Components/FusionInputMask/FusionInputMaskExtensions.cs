using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionInputMask (SetValue, FocusIn, Value).
    /// </summary>
    public static class FusionInputMaskExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        public static ComponentRef<FusionInputMask, TModel> SetValue<TModel>(
            this ComponentRef<FusionInputMask, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
