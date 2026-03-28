using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionSwitch"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionSwitch&gt;(m =&gt; m.IsActive).SetChecked(true)</c>.
    /// </remarks>
    public static class FusionSwitchExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        /// <summary>Sets the checked state of the switch.</summary>
        /// <param name="isChecked"><see langword="true"/> to check, <see langword="false"/> to uncheck.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionSwitch, TModel> SetChecked<TModel>(
            this ComponentRef<FusionSwitch, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("checked", coerce: "boolean"), value: isChecked ? "true" : "false");
        }

        /// <summary>Reads the current checked state for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionSwitch&gt;(m =&gt; m.IsActive).Value()).Eq(true).Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the switch's current checked state.</returns>
        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<FusionSwitch, TModel> self)
            where TModel : class
            => new TypedComponentSource<bool>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
