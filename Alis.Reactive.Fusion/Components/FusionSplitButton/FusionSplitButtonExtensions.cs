using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render mutations, reads, and methods for <see cref="FusionSplitButton"/>.
    /// </summary>
    public static class FusionSplitButtonExtensions
    {
        private static readonly ComponentProperty<string> ContentProperty =
            ComponentProperty<string>.Named("content");

        private static readonly ComponentProperty<bool> DisabledProperty =
            ComponentProperty<bool>.Named("disabled");

        private static readonly ComponentProperty<string> CssClassProperty =
            ComponentProperty<string>.Named("cssClass");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ToggleMethod =
            ComponentMethod.Named("toggle");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod RemoveItemsMethod =
            ComponentMethod.Named("removeItems").WithArgs<string[], bool>();

        /// <summary>Sets the visible primary button content and flushes the Syncfusion view.</summary>
        public static ComponentRef<FusionSplitButton, TModel> SetContent<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self,
            string content)
            where TModel : class
            => self
                .EmitSet(ContentProperty, ValueExpression.Literal(content))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether both SplitButton buttons are disabled and flushes the Syncfusion view.</summary>
        public static ComponentRef<FusionSplitButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Sets rendered CSS classes on the SplitButton wrapper and buttons, then flushes the Syncfusion view.</summary>
        public static ComponentRef<FusionSplitButton, TModel> SetCssClass<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self,
            string cssClass)
            where TModel : class
            => self
                .EmitSet(CssClassProperty, ValueExpression.Literal(cssClass))
                .EmitCall(DataBindMethod);

        /// <summary>Toggles the SplitButton secondary popup between open and closed states.</summary>
        public static ComponentRef<FusionSplitButton, TModel> Toggle<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self)
            where TModel : class
            => self.EmitCall(ToggleMethod);

        /// <summary>Moves focus into the SplitButton primary button through Syncfusion's public method.</summary>
        public static ComponentRef<FusionSplitButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes secondary action items by displayed text.</summary>
        public static ComponentRef<FusionSplitButton, TModel> RemoveItemsByText<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self,
            params string[] itemTexts)
            where TModel : class
            => self.EmitCall(RemoveItemsMethod, RemoveItemsArgs(itemTexts, isUniqueId: false));

        /// <summary>Removes secondary action items by Syncfusion item id.</summary>
        public static ComponentRef<FusionSplitButton, TModel> RemoveItemsById<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self,
            params string[] itemIds)
            where TModel : class
            => self.EmitCall(RemoveItemsMethod, RemoveItemsArgs(itemIds, isUniqueId: true));

        /// <summary>Reads the current Syncfusion content property.</summary>
        public static TypedComponentSource<string> Content<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self)
            where TModel : class
            => self.Read(ContentProperty);

        /// <summary>Reads whether the SplitButton is currently disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);

        /// <summary>Reads the current Syncfusion CSS class property.</summary>
        public static TypedComponentSource<string> CssClass<TModel>(
            this ComponentRef<FusionSplitButton, TModel> self)
            where TModel : class
            => self.Read(CssClassProperty);

        private static List<ValueExpression> RemoveItemsArgs(string[] items, bool isUniqueId)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var itemValues = items
                .Select(ValueExpression.Literal)
                .ToList();

            return new List<ValueExpression>
            {
                ValueExpression.Array(itemValues, Shape.ArrayOf(Shape.String)),
                ValueExpression.Literal(isUniqueId)
            };
        }
    }
}
