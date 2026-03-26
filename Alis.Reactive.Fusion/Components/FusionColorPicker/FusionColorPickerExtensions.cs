using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionColorPicker (SetValue, Toggle, Disable, Value).
    /// </summary>
    public static class FusionColorPickerExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

        /// <summary>Sets the color value (hex string, e.g. "#ff0000").</summary>
        public static ComponentRef<FusionColorPicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        /// <summary>Toggles the ColorPicker popup open/closed.</summary>
        public static ComponentRef<FusionColorPicker, TModel> Toggle<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("toggle"));

        /// <summary>Sets the disabled state of the ColorPicker.</summary>
        public static ComponentRef<FusionColorPicker, TModel> Disable<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self, bool disabled = true)
            where TModel : class
            => self.Emit(new SetPropMutation("disabled", coerce: "boolean"),
                value: disabled ? "true" : "false");

        /// <summary>Returns a typed source for the ColorPicker's current value (hex string).</summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionColorPicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
