using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionSwitch"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionSwitchExtensions
    {
        /// <summary>Sets the checked state of the switch.</summary>
        public static ComponentRef<FusionSwitch, TModel> SetChecked<TModel>(
            this ComponentRef<FusionSwitch, TModel> self, bool isChecked)
            where TModel : class
            => self.EmitSet("checked", ValueProducer.Literal(isChecked));
    }
}
