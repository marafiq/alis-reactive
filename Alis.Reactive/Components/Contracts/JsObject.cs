using System.Runtime.CompilerServices;

namespace Alis.Reactive
{
    /// <summary>
    /// Base for declaring a JavaScript object's members by the names its own API uses.
    /// </summary>
    /// <remarks>
    /// A <see cref="Component"/> is the JavaScript object the framework finds on an element it
    /// rendered. An event payload is the object an event hands its callback; its fields are plain
    /// C# properties, and it derives from <see cref="JsObject"/> only to declare methods:
    /// <code>
    /// public class FusionAutoCompleteFilteringArgs : JsObject
    /// {
    ///     public string Text { get; set; } = "";
    ///     public ComponentMethod&lt;IEnumerable&lt;object&gt;&gt; UpdateData =&gt;
    ///         Method&lt;IEnumerable&lt;object&gt;&gt;("updateData");
    /// }
    /// </code>
    /// Declare each member as a property. The C# property keys the member in the plan, so two
    /// properties can type two signatures of one JavaScript method:
    /// <code>
    /// public ComponentMethod&lt;int&gt; SelectIndex =&gt; Method&lt;int&gt;("select");
    /// public ComponentMethod&lt;int[]&gt; SelectIndexes =&gt; Method&lt;int[]&gt;("select");
    /// </code>
    /// A name may be a dotted path to a nested member, for example <c>"eventSettings.dataSource"</c>.
    /// </remarks>
    public abstract class JsObject
    {
        /// <summary>Declares a JavaScript property by the name the object uses.</summary>
        /// <typeparam name="TValue">The C# type of the property value.</typeparam>
        /// <param name="member">The JavaScript property name, for example <c>"min"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentProperty<TValue> Property<TValue>(
            string member, [CallerMemberName] string declaredBy = "") =>
            ComponentProperty<TValue>.Mapped(KeyOf(declaredBy), member);

        /// <summary>Declares a JavaScript method without arguments by the name the object uses.</summary>
        /// <param name="member">The JavaScript method name, for example <c>"increment"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentMethod Method(string member, [CallerMemberName] string declaredBy = "") =>
            ComponentMethod.Mapped(KeyOf(declaredBy), member);

        /// <summary>Declares a JavaScript method that takes one argument.</summary>
        /// <typeparam name="TArg">The C# type of the argument.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"updateData"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentMethod<TArg> Method<TArg>(string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentMethod<TArg>(ComponentMethod.Mapped(KeyOf(declaredBy), member).WithArgs<TArg>());

        /// <summary>Declares a JavaScript method that takes two arguments.</summary>
        /// <typeparam name="T1">The C# type of the first argument.</typeparam>
        /// <typeparam name="T2">The C# type of the second argument.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"addIcon"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentMethod<T1, T2> Method<T1, T2>(string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentMethod<T1, T2>(ComponentMethod.Mapped(KeyOf(declaredBy), member).WithArgs<T1, T2>());

        /// <summary>Declares a JavaScript method that takes three arguments.</summary>
        /// <typeparam name="T1">The C# type of the first argument.</typeparam>
        /// <typeparam name="T2">The C# type of the second argument.</typeparam>
        /// <typeparam name="T3">The C# type of the third argument.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"selectItems"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentMethod<T1, T2, T3> Method<T1, T2, T3>(
            string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentMethod<T1, T2, T3>(ComponentMethod.Mapped(KeyOf(declaredBy), member).WithArgs<T1, T2, T3>());

        /// <summary>Declares a JavaScript method without arguments that returns a value.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"getSelectedRecords"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentFunction<TResult> Function<TResult>(
            string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentFunction<TResult>(ComponentMethod.Mapped(KeyOf(declaredBy), member));

        /// <summary>Declares a JavaScript method that takes one argument and returns a value.</summary>
        /// <typeparam name="TArg">The C# type of the argument.</typeparam>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="member">The JavaScript method name.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentFunction<TArg, TResult> Function<TArg, TResult>(
            string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentFunction<TArg, TResult>(ComponentMethod.Mapped(KeyOf(declaredBy), member).WithArgs<TArg>());

        /// <summary>Declares a JavaScript method that takes two arguments and returns a value.</summary>
        /// <typeparam name="T1">The C# type of the first argument.</typeparam>
        /// <typeparam name="T2">The C# type of the second argument.</typeparam>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"getActualIndex"</c>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# property that declares the member.</param>
        protected static ComponentFunction<T1, T2, TResult> Function<T1, T2, TResult>(
            string member, [CallerMemberName] string declaredBy = "") =>
            new ComponentFunction<T1, T2, TResult>(ComponentMethod.Mapped(KeyOf(declaredBy), member).WithArgs<T1, T2>());

        // The plan keys a member by its C# property, camel-cased, so a member declared with its
        // JavaScript name keeps that name as its key.
        private static string KeyOf(string declaredBy)
        {
            if (string.IsNullOrEmpty(declaredBy))
                throw new System.ArgumentException(
                    "Declare a member as a property, for example public ComponentMethod Show => Method(\"show\");",
                    nameof(declaredBy));

            return char.ToLowerInvariant(declaredBy[0]) + declaredBy.Substring(1);
        }
    }
}
