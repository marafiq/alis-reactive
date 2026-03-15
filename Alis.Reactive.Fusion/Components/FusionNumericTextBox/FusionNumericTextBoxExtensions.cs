using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionNumericTextBox (SetValue, SetMin, FocusIn, etc.).
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value", coerce: "number"),
                value: value.ToString(CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("min", coerce: "number"),
                value: min.ToString(CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("increment"));

        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("decrement"));

        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => new TypedComponentSource<decimal>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
