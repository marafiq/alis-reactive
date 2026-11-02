using System;

namespace Alis.Reactive.Native
{
    /// <summary>
    /// An option of a native check list: the value a form post carries, the text beside the checkbox
    /// and, when the option has one, a description under the text.
    /// </summary>
    /// <remarks>
    /// Created in the controller and passed to <see cref="NativeCheckListBuilder{TModel,TProp}.Items"/>.
    /// </remarks>
    public sealed class CheckListItem
    {
        /// <summary>Option value written to form posts and component reads.</summary>
        public string Value { get; }

        /// <summary>Display text shown next to the checkbox.</summary>
        public string Text { get; }

        /// <summary>
        /// Secondary text shown below the display text. Null when the option has none; the list then
        /// renders no description element for it.
        /// </summary>
        public string? Description { get; }

        /// <summary>Creates an option without secondary text.</summary>
        /// <param name="value">Submitted option value.</param>
        /// <param name="text">Display text.</param>
        public CheckListItem(string value, string text)
        {
            Value = value;
            Text = text;
        }

        /// <summary>Creates an option with secondary text.</summary>
        /// <param name="value">Submitted option value.</param>
        /// <param name="text">Display text.</param>
        /// <param name="description">Secondary text.</param>
        public CheckListItem(string value, string text, string description)
            : this(value, text)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}
