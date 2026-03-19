using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionSwitch (SetChecked, Value).
    /// </summary>
    public static class FusionSwitchExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        public static ComponentRef<FusionSwitch, TModel> SetChecked<TModel>(
            this ComponentRef<FusionSwitch, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("checked", coerce: "boolean"), value: isChecked ? "true" : "false");
        }

        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<FusionSwitch, TModel> self)
            where TModel : class
            => new TypedComponentSource<bool>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
