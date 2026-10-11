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
    /// </remarks>
    public abstract class JsObject
    {
        /// <summary>Declares a JavaScript property by the name the object uses.</summary>
        /// <typeparam name="TValue">The C# type of the property value.</typeparam>
        /// <param name="member">The JavaScript property name, for example <c>"min"</c>.</param>
        protected static ComponentProperty<TValue> Property<TValue>(string member) =>
            ComponentProperty<TValue>.Named(member);

        /// <summary>Declares a JavaScript method without arguments by the name the object uses.</summary>
        /// <param name="member">The JavaScript method name, for example <c>"increment"</c>.</param>
        protected static ComponentMethod Method(string member) =>
            ComponentMethod.Named(member);

        /// <summary>Declares a JavaScript method that takes one argument.</summary>
        /// <typeparam name="TArg">The C# type of the argument.</typeparam>
        /// <param name="member">The JavaScript method name, for example <c>"updateData"</c>.</param>
        protected static ComponentMethod<TArg> Method<TArg>(string member) =>
            new ComponentMethod<TArg>(ComponentMethod.Named(member).WithArgs<TArg>());
    }
}
