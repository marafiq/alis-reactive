using System;
using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for <see cref="FusionListBox"/>.
    /// </summary>
    public static class FusionListBoxExtensions
    {
        private static readonly ComponentProperty<string[]> ValueProperty =
            ComponentProperty<string[]>.Named("value");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod SelectValuesMethod =
            ComponentMethod.Mapped("selectValues", "selectItems").WithArgs<string[], bool, bool>();

        private static readonly ComponentMethod UnselectValuesMethod =
            ComponentMethod.Mapped("unselectValues", "selectItems").WithArgs<string[], bool, bool>();

        private static readonly ComponentMethod SelectAllMethod =
            ComponentMethod.Named("selectAll").WithArgs<bool>();

        private static readonly ComponentMethod UnselectAllMethod =
            ComponentMethod.Mapped("unselectAll", "selectAll").WithArgs<bool>();

        private static readonly ComponentMethod EnableValuesMethod =
            ComponentMethod.Mapped("enableValues", "enableItems").WithArgs<string[], bool, bool>();

        private static readonly ComponentMethod DisableValuesMethod =
            ComponentMethod.Mapped("disableValues", "enableItems").WithArgs<string[], bool, bool>();

        /// <summary>Sets selected string values. Use an empty array to clear selection.</summary>
        public static ComponentRef<FusionListBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionListBox, TModel> self,
            string[] value)
            where TModel : class
            => self.EmitSet(ValueProperty, StringArray(value));

        /// <summary>Flushes pending property changes through the Syncfusion ListBox instance.</summary>
        public static ComponentRef<FusionListBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionListBox, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Selects items by string value.</summary>
        public static ComponentRef<FusionListBox, TModel> SelectValues<TModel>(
            this ComponentRef<FusionListBox, TModel> self,
            params string[] values)
            where TModel : class
            => self.EmitCall(
                SelectValuesMethod,
                new List<ValueExpression>
                {
                    StringArray(values),
                    ValueExpression.Literal(true),
                    ValueExpression.Literal(true)
                });

        /// <summary>Clears selection for items by string value.</summary>
        public static ComponentRef<FusionListBox, TModel> UnselectValues<TModel>(
            this ComponentRef<FusionListBox, TModel> self,
            params string[] values)
            where TModel : class
            => self.EmitCall(
                UnselectValuesMethod,
                new List<ValueExpression>
                {
                    StringArray(values),
                    ValueExpression.Literal(false),
                    ValueExpression.Literal(true)
                });

        /// <summary>Selects all enabled ListBox items.</summary>
        public static ComponentRef<FusionListBox, TModel> SelectAll<TModel>(
            this ComponentRef<FusionListBox, TModel> self)
            where TModel : class
            => self.EmitCall(
                SelectAllMethod,
                new List<ValueExpression> { ValueExpression.Literal(true) });

        /// <summary>Clears all ListBox selections.</summary>
        public static ComponentRef<FusionListBox, TModel> UnselectAll<TModel>(
            this ComponentRef<FusionListBox, TModel> self)
            where TModel : class
            => self.EmitCall(
                UnselectAllMethod,
                new List<ValueExpression> { ValueExpression.Literal(false) });

        /// <summary>Enables items by string value.</summary>
        public static ComponentRef<FusionListBox, TModel> EnableValues<TModel>(
            this ComponentRef<FusionListBox, TModel> self,
            params string[] values)
            where TModel : class
            => self.EmitCall(
                EnableValuesMethod,
                new List<ValueExpression>
                {
                    StringArray(values),
                    ValueExpression.Literal(true),
                    ValueExpression.Literal(true)
                });

        /// <summary>Disables items by string value.</summary>
        public static ComponentRef<FusionListBox, TModel> DisableValues<TModel>(
            this ComponentRef<FusionListBox, TModel> self,
            params string[] values)
            where TModel : class
            => self.EmitCall(
                DisableValuesMethod,
                new List<ValueExpression>
                {
                    StringArray(values),
                    ValueExpression.Literal(false),
                    ValueExpression.Literal(true)
                });

        /// <summary>Reads selected string values for use in conditions or gather.</summary>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<FusionListBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);

        private static ValueExpression StringArray(string[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return ValueExpression.LiteralRaw(values, Shape.ArrayOf(Shape.String));
        }
    }
}
