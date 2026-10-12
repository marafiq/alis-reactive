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
    ///     public ComponentProperty&lt;double&gt; Value =&gt; Property&lt;double&gt;("value");
    ///     public TypedEvent&lt;FusionRatingValueChangedArgs&gt; ValueChanged =&gt;
    ///         Event&lt;FusionRatingValueChangedArgs&gt;("valueChanged");
    ///
    ///     ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Value, committedBy: ValueChanged);
    /// }
    /// </code>
    /// </example>
    public sealed class ComponentInput
    {
        private ComponentInput(MemberName valueMember, Path valuePath, Shape valueShape, EventName committedBy)
        {
            ValueMember = valueMember;
            ValuePath = valuePath;
            ValueShape = valueShape;
            CommittedBy = committedBy;
        }

        internal MemberName ValueMember { get; }
        internal Path ValuePath { get; }

        /// <summary>The value's type as the component holds it; the model's type is applied where the model reads it.</summary>
        internal Shape ValueShape { get; }

        internal EventName CommittedBy { get; }

        /// <summary>Declares the input's value property and the event that commits it.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <typeparam name="TArgs">The payload of the committing event.</typeparam>
        /// <param name="value">The property that holds the value, declared on the component.</param>
        /// <param name="committedBy">The event the component raises when the person commits a value.</param>
        public static ComponentInput Of<TValue, TArgs>(ComponentProperty<TValue> value, TypedEvent<TArgs> committedBy)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (committedBy == null) throw new ArgumentNullException(nameof(committedBy));
            return new ComponentInput(MemberName.Of(value.Member), value.Path, value.Shape, EventName.Of(committedBy.ObjectEvent));
        }

        internal bool Matches(ComponentInput other) =>
            ValueMember.Equals(other.ValueMember)
            && ValuePath.ToString() == other.ValuePath.ToString()
            && CommittedBy.Equals(other.CommittedBy);

        internal string Describe() => $"{ValueMember.Value} ({ValuePath}), committed by {CommittedBy.Value}";
    }
}
