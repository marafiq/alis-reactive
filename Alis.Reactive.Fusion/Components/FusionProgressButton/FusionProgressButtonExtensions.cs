using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates rendered <see cref="FusionProgressButton"/> state from a Reactive Plan pipeline.
    /// </summary>
    public static class FusionProgressButtonExtensions
    {
        private static readonly ComponentProperty<string> ContentProperty =
            ComponentProperty<string>.Named("content");

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentProperty<string> CssClassProperty =
            ComponentProperty<string>.Named("cssClass");

        private static readonly ComponentProperty<bool> EnableProgressProperty =
            ComponentProperty<bool>.Named("enableProgress");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod StartMethod =
            ComponentMethod.Named("start");

        private static readonly ComponentMethod StartAtMethod =
            ComponentMethod.Mapped("startAt", "start").WithArgs<double>();

        private static readonly ComponentMethod ProgressCompleteMethod =
            ComponentMethod.Named("progressComplete");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets the visible progress button content.</summary>
        public static ComponentRef<FusionProgressButton, TModel> SetContent<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self,
            string content)
            where TModel : class
            => self
                .EmitSet(ContentProperty, ValueExpression.Literal(content))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the rendered progress button is disabled.</summary>
        public static ComponentRef<FusionProgressButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Sets rendered CSS classes on the progress button.</summary>
        public static ComponentRef<FusionProgressButton, TModel> SetCssClass<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self,
            string cssClass)
            where TModel : class
            => self
                .EmitSet(CssClassProperty, ValueExpression.Literal(cssClass))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the rendered progress filler is enabled.</summary>
        public static ComponentRef<FusionProgressButton, TModel> SetProgressEnabled<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self,
            bool enabled)
            where TModel : class
            => self
                .EmitSet(EnableProgressProperty, ValueExpression.Literal(enabled))
                .EmitCall(DataBindMethod);

        /// <summary>Starts progress from the current ProgressButton percent.</summary>
        public static ComponentRef<FusionProgressButton, TModel> Start<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.EmitCall(StartMethod);

        /// <summary>Starts progress from the supplied percent.</summary>
        public static ComponentRef<FusionProgressButton, TModel> StartAt<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self,
            double percent)
            where TModel : class
            => self.EmitCall(
                StartAtMethod,
                new List<ValueExpression> { ValueExpression.Literal(percent) });

        /// <summary>Completes the current progress operation.</summary>
        public static ComponentRef<FusionProgressButton, TModel> ProgressComplete<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.EmitCall(ProgressCompleteMethod);

        /// <summary>Moves focus into the rendered progress button.</summary>
        public static ComponentRef<FusionProgressButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads the rendered progress button content.</summary>
        public static TypedComponentSource<string> Content<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.Read(ContentProperty);

        /// <summary>Reads whether the rendered progress button is disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);

        /// <summary>Reads the rendered progress button CSS classes.</summary>
        public static TypedComponentSource<string> CssClass<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.Read(CssClassProperty);

        /// <summary>Reads whether the rendered progress filler is enabled.</summary>
        public static TypedComponentSource<bool> ProgressEnabled<TModel>(
            this ComponentRef<FusionProgressButton, TModel> self)
            where TModel : class
            => self.Read(EnableProgressProperty);
    }
}
