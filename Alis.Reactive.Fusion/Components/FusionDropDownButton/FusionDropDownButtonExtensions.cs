using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render component operations and reads for <see cref="FusionDropDownButton"/>.
    /// </summary>
    public static class FusionDropDownButtonExtensions
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

        /// <summary>Sets the visible dropdown button content.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> SetContent<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self,
            string content)
            where TModel : class
            => self
                .EmitSet(ContentProperty, ValueExpression.Literal(content))
                .EmitCall(DataBindMethod);

        /// <summary>Sets whether the rendered dropdown button is disabled.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> SetDisabled<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self,
            bool disabled)
            where TModel : class
            => self
                .EmitSet(DisabledProperty, ValueExpression.Literal(disabled))
                .EmitCall(DataBindMethod);

        /// <summary>Sets the rendered CSS classes on the dropdown button and popup.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> SetCssClass<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self,
            string cssClass)
            where TModel : class
            => self
                .EmitSet(CssClassProperty, ValueExpression.Literal(cssClass))
                .EmitCall(DataBindMethod);

        /// <summary>Toggles the DropDownButton popup between open and closed states.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> Toggle<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self)
            where TModel : class
            => self.EmitCall(ToggleMethod);

        /// <summary>Moves focus into the rendered dropdown button.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes action items by displayed text.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> RemoveItemsByText<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self,
            params string[] itemTexts)
            where TModel : class
            => self.EmitCall(RemoveItemsMethod, RemoveItemsArgs(itemTexts, isUniqueId: false));

        /// <summary>Removes action items by Syncfusion item id.</summary>
        public static ComponentRef<FusionDropDownButton, TModel> RemoveItemsById<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self,
            params string[] itemIds)
            where TModel : class
            => self.EmitCall(RemoveItemsMethod, RemoveItemsArgs(itemIds, isUniqueId: true));

        /// <summary>Reads the current rendered dropdown button content.</summary>
        public static TypedComponentSource<string> Content<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self)
            where TModel : class
            => self.Read(ContentProperty);

        /// <summary>Reads whether the dropdown button is currently disabled.</summary>
        public static TypedComponentSource<bool> Disabled<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self)
            where TModel : class
            => self.Read(DisabledProperty);

        /// <summary>Reads the current rendered dropdown button CSS classes.</summary>
        public static TypedComponentSource<string> CssClass<TModel>(
            this ComponentRef<FusionDropDownButton, TModel> self)
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
