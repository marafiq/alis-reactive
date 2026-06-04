using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan operations for <see cref="NativeButton"/> text content and focus.
    /// </summary>
    public static class NativeButtonExtensions
    {
        private static readonly ComponentProperty<string> TextContentProperty =
            ComponentProperty<string>.Named("textContent");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        /// <summary>
        /// Sets the button text content through the component contract.
        /// </summary>
        /// <typeparam name="TModel">The view model type for the component reference.</typeparam>
        /// <param name="self">The button component reference.</param>
        /// <param name="text">The text content to write to the button.</param>
        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.EmitSet(TextContentProperty, ValueExpression.Literal(text));
        }

        /// <summary>
        /// Moves keyboard focus into the button.
        /// </summary>
        /// <typeparam name="TModel">The view model type for the component reference.</typeparam>
        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }
    }
}
