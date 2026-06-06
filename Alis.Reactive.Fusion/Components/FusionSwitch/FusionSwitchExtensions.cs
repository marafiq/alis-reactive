using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionSwitch"/> values from a Reactive Plan pipeline.
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

        /// <summary>Reads the checked state for conditions or gather.</summary>
        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<FusionSwitch, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
