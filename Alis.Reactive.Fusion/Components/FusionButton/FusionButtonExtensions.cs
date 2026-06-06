using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render component operations and reads for <see cref="FusionButton"/>.
    /// </summary>
    public static class FusionButtonExtensions
    {
        private static readonly ComponentProperty<string> ContentProperty =
            ComponentProperty<string>.Named("content");

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentProperty<string> IconCssProperty =
            ComponentProperty<string>.Named("iconCss");

        private static readonly ComponentProperty<string> IconPositionProperty =
            ComponentProperty<string>.Named("iconPosition");

        private static readonly ComponentProperty<string> CssClassProperty =
            ComponentProperty<string>.Named("cssClass");

        private static readonly ComponentProperty<bool> IsPrimaryProperty =
            ComponentProperty<bool>.Named("isPrimary");

        private static readonly ComponentProperty<bool> IsToggleProperty =
            ComponentProperty<bool>.Named("isToggle");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ClickMethod =
            ComponentMethod.Named("click");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets the visible button content.</summary>
        public static ComponentRef<FusionButton, TModel> SetContent<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            string content)
            where TModel : class
            => self
                .EmitSet(ContentProperty, ValueExpression.Literal(content))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the rendered button is disabled.</summary>
        public static ComponentRef<FusionButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Sets the rendered button icon CSS and position.</summary>
        public static ComponentRef<FusionButton, TModel> SetIcon<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            string iconCss,
            FusionButtonIconPosition position)
            where TModel : class
            => self
                .EmitSet(IconCssProperty, ValueExpression.Literal(iconCss))
                .EmitSet(IconPositionProperty, ValueExpression.Literal(ToSyncfusion(position)))
                .EmitCall(DataBindMethod);

        /// <summary>Sets the rendered button CSS classes.</summary>
        public static ComponentRef<FusionButton, TModel> SetCssClass<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            string cssClass)
            where TModel : class
            => self
                .EmitSet(CssClassProperty, ValueExpression.Literal(cssClass))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the button has Syncfusion primary styling.</summary>
        public static ComponentRef<FusionButton, TModel> SetPrimary<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            bool isPrimary)
            where TModel : class
            => self
                .EmitSet(IsPrimaryProperty, ValueExpression.Literal(isPrimary))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the button toggles active state when clicked.</summary>
        public static ComponentRef<FusionButton, TModel> SetToggle<TModel>(
            this ComponentRef<FusionButton, TModel> self,
            bool isToggle)
            where TModel : class
            => self
                .EmitSet(IsToggleProperty, ValueExpression.Literal(isToggle))
                .EmitCall(DataBindMethod);

        /// <summary>Invokes the rendered button click.</summary>
        public static ComponentRef<FusionButton, TModel> Click<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.EmitCall(ClickMethod);

        /// <summary>Moves focus into the rendered button.</summary>
        public static ComponentRef<FusionButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads the current rendered button content.</summary>
        public static TypedComponentSource<string> Content<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.Read(ContentProperty);

        /// <summary>Reads whether the button is currently disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);

        /// <summary>Reads the current CSS class property.</summary>
        public static TypedComponentSource<string> CssClass<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.Read(CssClassProperty);

        /// <summary>Reads whether the button is currently primary.</summary>
        public static TypedComponentSource<bool> IsPrimary<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.Read(IsPrimaryProperty);

        /// <summary>Reads whether the button currently toggles active state.</summary>
        public static TypedComponentSource<bool> IsToggle<TModel>(
            this ComponentRef<FusionButton, TModel> self)
            where TModel : class
            => self.Read(IsToggleProperty);

        private static string ToSyncfusion(FusionButtonIconPosition position) =>
            position switch
            {
                FusionButtonIconPosition.Left => "Left",
                FusionButtonIconPosition.Right => "Right",
                FusionButtonIconPosition.Top => "Top",
                FusionButtonIconPosition.Bottom => "Bottom",
                _ => throw new System.ArgumentOutOfRangeException(nameof(position), position, null)
            };
    }
}
