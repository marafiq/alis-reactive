using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionSmartTextAreaExtensions
    {
        private static readonly FusionSmartTextArea Component = new FusionSmartTextArea();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        public static ComponentRef<FusionSmartTextArea, TModel> SetValue<TModel>(
            this ComponentRef<FusionSmartTextArea, TModel> self,
            string value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueExpression.Literal(value));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionSmartTextArea, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        public static ComponentRef<FusionSmartTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<FusionSmartTextArea, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);
    }
}
