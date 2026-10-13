using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Base for declaring a JavaScript object by its own API: each property, method and event it has, by
    /// the name the object uses. A page then uses the object as the object: <c>rate.Min.Set(0m)</c>,
    /// <c>grid.Refresh()</c>, <c>p.When(args.Value).Gt(100m)</c>.
    /// </summary>
    /// <remarks>
    /// <para>Declare each member as a C# member; the C# name, camel-cased, is the JavaScript name unless one is given:</para>
    /// <code>
    /// public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    /// {
    ///     public Prop&lt;decimal&gt; Value =&gt; Prop&lt;decimal&gt;();                 // read and written
    ///     public ReadOnly&lt;bool&gt; Enabled =&gt; ReadOnly&lt;bool&gt;();             // read
    ///     public void Increment() =&gt; Call();                                   // a method
    ///     public void AddIcon(Arg&lt;string&gt; position, Arg&lt;string&gt; icons) =&gt; Call(position, icons);
    ///     public TypedSource&lt;string&gt; GetText() =&gt; Invoke&lt;string&gt;();           // a method that returns a value
    ///     public void ShowColumn(Field column) =&gt; Call(column, Fixed("field"), name: "showColumns");
    ///     public void SaveDraft() =&gt; CallAsync();                              // returns a promise the next steps wait for
    /// }
    /// </code>
    /// <para>
    /// A declaration is live where a page reaches it: <c>p.Component&lt;T&gt;(..)</c>, the object an event
    /// hands its callback, or a registered plugin. A write or a call lands in the callback the line is
    /// written in, whichever variable holds the object.
    /// </para>
    /// </remarks>
    public abstract class JsObject
    {
        private ObjectBinding? _binding;

        // A nested object (Object<T>) is reached through the object that holds it: it shares that object's
        // binding, and its members' JavaScript names start with the property that holds it.
        private JsObject? _holder;
        private string _pathPrefix = "";

        internal ObjectBinding Binding => _binding ?? _holder?.Binding ?? throw new InvalidOperationException(
            $"'{GetType().Name}' is a declaration, not a live object. Reach one inside a callback: " +
            $"p.Component<{GetType().Name}>(..), the payload an event hands its callback, or a registered plugin.");

        internal void BindTo(ObjectBinding binding) =>
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));

        /// <summary>Declares a property the page reads.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased; a dotted path for a nested property.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected ReadOnly<TValue> ReadOnly<TValue>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new ReadOnly<TValue>(this, ComponentProperty<TValue>.At(JavaScriptPath(name, declaredBy)));

        /// <summary>Declares a property the page reads and writes.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased; a dotted path for a nested property.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected Prop<TValue> Prop<TValue>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new Prop<TValue>(this, ComponentProperty<TValue>.At(JavaScriptPath(name, declaredBy)));

        /// <summary>
        /// Declares a property that holds an object with members of its own, declared as its own class:
        /// <c>public FusionScheduleEventSettings EventSettings =&gt; Object&lt;FusionScheduleEventSettings&gt;();</c>,
        /// used as <c>schedule.EventSettings.DataSource.Set(..)</c>.
        /// </summary>
        /// <typeparam name="TObject">The declaration of the object the property holds.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected TObject Object<TObject>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            new TObject { _holder = this, _pathPrefix = JavaScriptPath(name, declaredBy) + "." };

        /// <summary>
        /// Calls a method that returns an object with members of its own, declared as its own class:
        /// <c>public CodeModel GetModel() =&gt; InvokeObject&lt;CodeModel&gt;();</c>. Each use of the returned object
        /// calls the method again, where the use is written.
        /// </summary>
        /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TObject InvokeObject<TObject>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            Returned<TObject>(name, declaredBy);

        /// <summary>
        /// Calls a method that returns an object with members of its own, declared as its own class:
        /// <c>public FusionGridColumn GetColumnByField(Field field) =&gt; InvokeObject&lt;FusionGridColumn&gt;(field);</c>,
        /// used as <c>grid.GetColumnByField(..).HeaderText.Set(..)</c>. Each use of the returned object calls
        /// the method again, where the use is written.
        /// </summary>
        /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TObject InvokeObject<TObject>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            Returned<TObject>(name, declaredBy, first);

        /// <summary>Calls the method the C# member declares.</summary>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Call(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: false);

        /// <summary>Calls the method the C# member declares with one argument.</summary>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Call(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: false, first);

        /// <summary>Calls the method the C# member declares with two arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Call(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: false, first, second);

        /// <summary>Calls the method the C# member declares with three arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Call(Arg first, Arg second, Arg third, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: false, first, second, third);

        /// <summary>Calls the method the C# member declares with four arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="fourth">The fourth argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Call(Arg first, Arg second, Arg third, Arg fourth, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: false, first, second, third, fourth);

        /// <summary>Calls a method that returns a promise; the steps after it wait for it to settle.</summary>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void CallAsync(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: true);

        /// <summary>Calls a method that returns a promise with one argument; the steps after it wait for it.</summary>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void CallAsync(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: true, first);

        /// <summary>Calls a method that returns a promise with two arguments; the steps after it wait for it.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void CallAsync(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Emit(name, declaredBy, awaits: true, first, second);

        /// <summary>Reads what the method the C# member declares returns.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="name">
        /// The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is
        /// itself a function.
        /// </param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Invoke<TResult>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: false);

        /// <summary>Reads what the method returns for one argument.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">
        /// The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is
        /// itself a function.
        /// </param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Invoke<TResult>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: false, first);

        /// <summary>Reads what the method returns for two arguments.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">
        /// The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is
        /// itself a function.
        /// </param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Invoke<TResult>(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: false, first, second);

        /// <summary>Reads what the method returns for three arguments.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="name">
        /// The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is
        /// itself a function.
        /// </param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Invoke<TResult>(Arg first, Arg second, Arg third, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: false, first, second, third);

        /// <summary>Reads the value a method's promise settles with; the step that reads it waits for the promise.</summary>
        /// <typeparam name="TResult">The C# type of the value the promise settles with.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> InvokeAsync<TResult>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: true);

        /// <summary>Reads the value a method's promise settles with, for one argument.</summary>
        /// <typeparam name="TResult">The C# type of the value the promise settles with.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> InvokeAsync<TResult>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Read<TResult>(name, declaredBy, awaits: true, first);

        /// <summary>
        /// An argument the method always takes, fixed by the declaration rather than by the caller, for
        /// example the kind of key a column method is given: <c>Call(column, Fixed("field"))</c>.
        /// </summary>
        /// <param name="value">The literal, written into the plan.</param>
        protected static Arg Fixed(object value) => Arg.Fixed(value);

        /// <summary>
        /// Passes a value of any type the declared parameter type accepts, for a method that takes, say, items
        /// where the page passes its own list: <c>Call(Pass&lt;IEnumerable&lt;object&gt;, TItems&gt;(items))</c>.
        /// </summary>
        /// <typeparam name="TDeclared">The type the JavaScript method declares.</typeparam>
        /// <typeparam name="TSource">The value's type.</typeparam>
        /// <param name="value">The value, read when the step runs.</param>
        protected static Arg Pass<TDeclared, TSource>(TypedSource<TSource> value) where TSource : TDeclared =>
            Arg.Declared<TDeclared, TSource>(value);

        private void Emit(string? name, string declaredBy, bool awaits, params Arg[] args) =>
            Binding.Call(MethodNamed(name, declaredBy, awaits, args), Expressions(args));

        private TypedSource<TResult> Read<TResult>(string? name, string declaredBy, bool awaits, params Arg[] args) =>
            new MethodRead<TResult>(this, MethodNamed(name, declaredBy, awaits, args), Expressions(args));

        private TObject Returned<TObject>(string? name, string declaredBy, params Arg[] args) where TObject : JsObject, new()
        {
            var returned = new TObject();
            returned.BindTo(new ReturnedObjectBinding(Binding, MethodNamed(name, declaredBy, awaits: false, args), Expressions(args)));
            return returned;
        }

        private ComponentMethod MethodNamed(string? name, string declaredBy, bool awaits, Arg[] args) =>
            ComponentMethod.Of(declaredBy, JavaScriptPath(name, declaredBy), args.Select(arg => arg.Shape).ToList(), awaits);

        // The member's JavaScript path from the object a page holds: the holder's property first for a
        // nested object, then the declared name, or the C# name camel-cased.
        private string JavaScriptPath(string? name, string declaredBy) => _pathPrefix + (name ?? JavaScriptNameOf(declaredBy));

        private static IReadOnlyList<ValueExpression> Expressions(Arg[] args) =>
            args.Select(arg => arg.Expression).ToList();

        // A member is named by its C# name, camel-cased, unless the declaration names it.
        private static string JavaScriptNameOf(string declaredBy)
        {
            if (string.IsNullOrEmpty(declaredBy))
                throw new ArgumentException("Declare a member as a C# property or method, for example public void Show() => Call();", nameof(declaredBy));

            return JsonMemberName.Of(declaredBy);
        }

        // A method's result is read where the value is used, in the callback that uses it.
        private sealed class MethodRead<TResult> : TypedSource<TResult>
        {
            private readonly JsObject _owner;
            private readonly ComponentMethod _method;
            private readonly IReadOnlyList<ValueExpression> _args;

            internal MethodRead(JsObject owner, ComponentMethod method, IReadOnlyList<ValueExpression> args)
            {
                _owner = owner;
                _method = method;
                _args = args;
            }

            internal override ValueExpression Expression => _owner.Binding.Invoke(_method, _args, Shape);
        }
    }
}
