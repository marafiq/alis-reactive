using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionSmartPasteButtonExtensions
    {
        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentMethod ClickMethod =
            ComponentMethod.Named("click");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        public static ComponentRef<FusionSmartPasteButton, TModel> Click<TModel>(
            this ComponentRef<FusionSmartPasteButton, TModel> self)
            where TModel : class
            => self.EmitCall(ClickMethod);

        public static ComponentRef<FusionSmartPasteButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionSmartPasteButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        public static ComponentRef<FusionSmartPasteButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionSmartPasteButton, TModel> self,
            bool disabled)
            where TModel : class
            => self.EmitSet(DisabledProperty, ValueExpression.Literal(disabled));

        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionSmartPasteButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);
    }
}
