using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionComboBox (SetValue, FocusIn, ShowPopup, etc.).
    /// </summary>
    public static class FusionComboBoxExtensions
    {
        private static readonly FusionComboBox Component = new FusionComboBox();

        public static ComponentRef<FusionComboBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionComboBox, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionComboBox, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        public static ComponentRef<FusionComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionComboBox, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
