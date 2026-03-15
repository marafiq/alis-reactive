using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionDropDownList (SetValue, SetText, FocusIn, etc.).
    /// </summary>
    public static class FusionDropDownListExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        public static ComponentRef<FusionDropDownList, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
