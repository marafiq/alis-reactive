using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// What makes a component an input: the property that holds its value, and the event it raises
    /// when the person commits a value. Gather and validation read the value; a form checks the
    /// field again whenever the event fires, whatever the component calls it.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class FusionRating : FusionComponent, IInputComponent
    /// {
    ///     public Prop&lt;double&gt; Value =&gt; Prop&lt;double&gt;();
    ///     public TypedEvent&lt;FusionRatingValueChangedArgs&gt; ValueChanged =&gt;
    ///         Event&lt;FusionRatingValueChangedArgs&gt;();
    ///
    ///     ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Value, committedBy: ValueChanged);
    /// }
    /// </code>
    /// </example>
    public sealed class ComponentInput
    {
        private ComponentInput(Path valuePath, EventName committedBy)
        {
            ValuePath = valuePath;
            CommittedBy = committedBy;
        }

        /// <summary>The path from the component's object to its value; the model reads the value in the model's type.</summary>
        internal Path ValuePath { get; }

        internal EventName CommittedBy { get; }

        /// <summary>Declares the input's value property and the event that commits it.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <typeparam name="TArgs">The payload of the committing event.</typeparam>
        /// <param name="value">The property that holds the value, declared on the component.</param>
        /// <param name="committedBy">The event the component raises when the person commits a value.</param>
        public static ComponentInput Of<TValue, TArgs>(ReadOnly<TValue> value, Event<TArgs> committedBy)
            where TArgs : JsObject
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (committedBy == null) throw new ArgumentNullException(nameof(committedBy));
            return new ComponentInput(value.Member.Path, EventName.Of(committedBy.ObjectEvent));
        }

        internal bool Matches(ComponentInput other) =>
            ValuePath.ToString() == other.ValuePath.ToString()
            && CommittedBy.Equals(other.CommittedBy);

        internal string Describe() => $"{ValuePath}, committed by {CommittedBy.Value}";
    }
}
