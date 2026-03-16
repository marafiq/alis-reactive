using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionAutoComplete (SetValue, FocusIn, ShowPopup, etc.).
    /// </summary>
    public static class FusionAutoCompleteExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        public static ComponentRef<FusionAutoComplete, TModel> SetValue<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
