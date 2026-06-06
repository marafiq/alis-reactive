using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionSwitch"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionSwitchExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        private static readonly ComponentProperty<bool> ValueProperty =
            ComponentProperty<bool>.Named(Component.ValueMember);

        /// <summary>Sets the checked state of the switch.</summary>
        public static ComponentRef<FusionSwitch, TModel> SetChecked<TModel>(
            this ComponentRef<FusionSwitch, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(isChecked));
        }

        /// <summary>Reads the current checked state for use in conditions or gather.</summary>
        /// <returns>A typed source representing the switch's current checked state.</returns>
        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<FusionSwitch, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
