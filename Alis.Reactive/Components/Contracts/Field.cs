using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// A member of your row, item or card class, named the way a component's JavaScript API names it:
    /// <c>Field.Of((Resident r) =&gt; r.CareLevel)</c> is <c>"careLevel"</c>, the camel-case JSON name.
    /// </summary>
    /// <remarks>
    /// Pass a field wherever a declared member takes a field name, so the compiler checks the name
    /// and, through <see cref="Field{TValue}"/>, the value that goes with it:
    /// <code>
    /// grid.Call(c =&gt; c.SortColumn, Field.Of((Resident r) =&gt; r.Name), FusionGridSortDirection.Descending, false)
    /// grid.Call(c =&gt; c.SetCellValue&lt;int&gt;(), 6001, Field.Of((Resident r) =&gt; r.OpenTasks), 4)
    /// </code>
    /// A nested member is a dotted name: <c>r =&gt; r.Address.City</c> is <c>"address.city"</c>.
    /// </remarks>
    public abstract class Field
    {
        private protected Field(string name)
        {
            Name = name;
        }

        /// <summary>The name the component uses, for example <c>"careLevel"</c>.</summary>
        public string Name { get; }

        /// <summary>The field a member expression selects; the member's type travels with it.</summary>
        /// <typeparam name="TRow">Your row, item or card class.</typeparam>
        /// <typeparam name="TValue">The type of the selected member.</typeparam>
        /// <param name="member">The member, for example <c>(Resident r) =&gt; r.CareLevel</c>.</param>
        public static Field<TValue> Of<TRow, TValue>(Expression<Func<TRow, TValue>> member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var name = ExpressionPathHelper.ToEventPath(member);
            if (name.Length == 0)
                throw new ArgumentException("Select a member of the row, for example (Resident r) => r.Name.", nameof(member));

            return new Field<TValue>(name);
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>A <see cref="Field"/> whose member holds a <typeparamref name="TValue"/>.</summary>
    /// <typeparam name="TValue">The type of the member, which a value written to it must match.</typeparam>
    public sealed class Field<TValue> : Field
    {
        internal Field(string name) : base(name)
        {
        }
    }
}
