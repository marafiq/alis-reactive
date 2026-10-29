using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Base for declaring a JavaScript object by what it has: each property, method and event, by the
    /// name the object uses. A page then uses the object as the object: <c>rate.Min.Set(0m)</c>,
    /// <c>grid.Refresh()</c>, <c>p.When(args.Value).Gt(100m)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each member is a C# member of the kind the JavaScript member is, and the C# name, camel-cased, is
    /// the JavaScript name unless one is given:
    /// </para>
    /// <code>
    /// public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    /// {
    ///     public Property&lt;decimal&gt; Value =&gt; Property&lt;decimal&gt;();            // read and written
    ///     public ReadOnly&lt;bool&gt; Enabled =&gt; ReadOnly&lt;bool&gt;();                 // read
    ///     public WriteOnly&lt;string&gt; Pin =&gt; WriteOnly&lt;string&gt;();               // written only
    ///     public TextBoxModel Model =&gt; Object&lt;TextBoxModel&gt;();                 // an object with members of its own
    ///     public void Increment() =&gt; Method();                                    // a method that returns nothing
    ///     public void AddIcon(Arg&lt;string&gt; position, Arg&lt;string&gt; icons) =&gt; Method(position, icons);
    ///     public TypedSource&lt;string&gt; GetText() =&gt; Method&lt;string&gt;();            // a method that returns a value
    ///     public TextBoxModel GetModel() =&gt; Object&lt;TextBoxModel&gt;();             // a method that returns an object
    ///     public void ShowColumn(Field column) =&gt; Method(column, Fixed("field"), name: "showColumns");
    ///     public void SaveDraft() =&gt; MethodAsync();                                // returns a promise the next steps wait for
    ///     public Event&lt;FusionNumericTextBoxChangeArgs&gt; Change =&gt; Event&lt;FusionNumericTextBoxChangeArgs&gt;();
    /// }
    /// </code>
    /// <para>
    /// A declaration is live where a page reaches it: <c>p.Component&lt;T&gt;(..)</c>, the object an event
    /// hands its callback, or a registered plugin. A write or a call lands in the callback the line is
    /// written in, whichever variable holds the object. A method that returns a value runs in that
    /// callback too, once: before the first line that uses its result, or at its own line when no line does.
    /// An async result first used inside a branch's arm, a request's WhileLoading or Finally, a request its
    /// form checks, or a chained or parallel request is asked again by a line outside that part, which
    /// cannot know the part ran.
    /// </para>
    /// </remarks>
    public abstract class JsObject
    {
        private ObjectBinding? _binding;

        // A nested object (Object<T> on a property, or an item of Objects<T>) is reached through the object
        // that holds it: it shares that object's binding, and its members' JavaScript names start with the
        // path that holds it.
        private JsObject? _holder;
        private Path _holderPath = Path.None;

        // An object a method returned: reached through the method's result, so it holds no events.
        private bool _returned;

        internal ObjectBinding Binding => _binding ?? _holder?.Binding ?? throw new InvalidOperationException(
            $"'{GetType().Name}' is a declaration, not a live object. Reach one inside a callback: " +
            $"p.Component<{GetType().Name}>(..), the payload an event hands its callback, or a registered plugin.");

        internal void BindTo(ObjectBinding binding) =>
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));

        internal static TObject Nested<TObject>(JsObject holder, Path path) where TObject : JsObject, new() =>
            new TObject { _holder = holder, _holderPath = path };

        private bool IsReturned => _returned || (_holder?.IsReturned ?? false);

        // ── properties ───────────────────────────────────────────────────────────

        /// <summary>Declares a property the page reads.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased. A member of an object the component holds belongs to that object's declaration (<see cref="Object{TObject}()"/>); a field of each item of a list is read with <see cref="ListPropertyExtensions.Each{TItem, TField}"/>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected ReadOnly<TValue> ReadOnly<TValue>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new ReadOnly<TValue>(this, ComponentProperty<TValue>.At(JavaScriptPath(name, declaredBy)));

        /// <summary>Declares a property the page reads and writes.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased. A member of an object the component holds belongs to that object's declaration (<see cref="Object{TObject}()"/>); a field of each item of a list is read with <see cref="ListPropertyExtensions.Each{TItem, TField}"/>.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected Property<TValue> Property<TValue>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new Property<TValue>(this, ComponentProperty<TValue>.At(JavaScriptPath(name, declaredBy)));

        /// <summary>Declares a property the page writes and never reads: the object has a setter and no getter.</summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the property.</param>
        protected WriteOnly<TValue> WriteOnly<TValue>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new WriteOnly<TValue>(this, ComponentProperty<TValue>.At(JavaScriptPath(name, declaredBy)));

        // ── objects ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Declares an object with members of its own, declared as its own class. On a property it is the
        /// object the property holds: <c>public FusionScheduleEventSettings EventSettings =&gt; Object&lt;FusionScheduleEventSettings&gt;();</c>,
        /// used as <c>schedule.EventSettings.Events&lt;Shift&gt;().Set(..)</c>. On a method it is the object the
        /// method returns: <c>public ThermostatReading Reading() =&gt; Object&lt;ThermostatReading&gt;();</c>, used as
        /// <c>thermostat.Reading().Label()</c>; the method runs once, before the first line that uses what it returned.
        /// </summary>
        /// <typeparam name="TObject">The declaration of the object.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the object.</param>
        protected TObject Object<TObject>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            DeclaresMethod(declaredBy)
                ? ReturnedObject<TObject>(name, declaredBy)
                : Nested<TObject>(this, JavaScriptPath(name, declaredBy));

        /// <summary>
        /// Declares a method that returns an object with members of its own, for one argument:
        /// <c>public FusionGridColumn GetColumnByField(Field field) =&gt; Object&lt;FusionGridColumn&gt;(field);</c>,
        /// used as <c>grid.GetColumnByField(..).HeaderText.Set(..)</c>.
        /// </summary>
        /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TObject Object<TObject>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            ReturnedObject<TObject>(name, declaredBy, first);

        /// <summary>Declares a method that returns an object with members of its own, for two arguments.</summary>
        /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
        /// <param name="first">The first argument.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TObject Object<TObject>(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "")
            where TObject : JsObject, new() =>
            ReturnedObject<TObject>(name, declaredBy, first, second);

        /// <summary>
        /// Declares a list of objects with members of their own. On a property it is the list the property
        /// holds: <c>public Objects&lt;ThermostatZone&gt; Zones =&gt; Objects&lt;ThermostatZone&gt;();</c>, used as
        /// <c>thermostat.Zones.At(1).Boost()</c>, <c>thermostat.Zones.Count</c> and
        /// <c>thermostat.Zones.Each(zone =&gt; zone.Name)</c>. On a method it is the list the method returns.
        /// </summary>
        /// <typeparam name="TItem">The declaration of each item.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the list.</param>
        protected Objects<TItem> Objects<TItem>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TItem : JsObject, new() =>
            DeclaresMethod(declaredBy)
                ? new Objects<TItem>(ReturnedObject<ReturnedList>(name, declaredBy), Path.None)
                : new Objects<TItem>(this, JavaScriptPath(name, declaredBy));

        // ── methods that return nothing ──────────────────────────────────────────

        /// <summary>Declares a method that returns nothing and takes no argument: <c>public void Refresh() =&gt; Method();</c>.</summary>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Method(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: false);

        /// <summary>Declares a method that returns nothing, for one argument.</summary>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Method(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: false, first);

        /// <summary>Declares a method that returns nothing, for two arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Method(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: false, first, second);

        /// <summary>Declares a method that returns nothing, for three arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Method(Arg first, Arg second, Arg third, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: false, first, second, third);

        /// <summary>Declares a method that returns nothing, for four arguments.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="fourth">The fourth argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void Method(Arg first, Arg second, Arg third, Arg fourth, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: false, first, second, third, fourth);

        /// <summary>Declares a method that returns a promise and takes no argument; the steps after it wait for the promise to settle.</summary>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void MethodAsync(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: true);

        /// <summary>Declares a method that returns a promise, for one argument; the steps after it wait for the promise to settle.</summary>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void MethodAsync(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: true, first);

        /// <summary>Declares a method that returns a promise, for two arguments; the steps after it wait for the promise to settle.</summary>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected void MethodAsync(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Run(name, declaredBy, awaits: true, first, second);

        // ── methods that return a value ──────────────────────────────────────────

        /// <summary>
        /// Declares a method that returns a value and takes no argument:
        /// <c>public TypedSource&lt;int&gt; Remaining() =&gt; Method&lt;int&gt;();</c>. The page uses the result as a value;
        /// the method runs once, before the first line that uses it, or at its own line when no line does.
        /// </summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is itself a function.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Method<TResult>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: false);

        /// <summary>Declares a method that returns a value, for one argument.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased; <c>""</c> calls a plugin that is itself a function.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Method<TResult>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: false, first);

        /// <summary>Declares a method that returns a value, for two arguments.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Method<TResult>(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: false, first, second);

        /// <summary>Declares a method that returns a value, for three arguments.</summary>
        /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="third">The third argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> Method<TResult>(Arg first, Arg second, Arg third, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: false, first, second, third);

        /// <summary>
        /// Declares a method that returns a promise of a value and takes no argument:
        /// <c>public TypedSource&lt;string&gt; Summary() =&gt; MethodAsync&lt;string&gt;();</c>. The step that first uses the
        /// result waits for the promise.
        /// </summary>
        /// <typeparam name="TResult">The C# type of the settled value.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> MethodAsync<TResult>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: true);

        /// <summary>Declares a method that returns a promise of a value, for one argument; the step that first uses the result waits for the promise.</summary>
        /// <typeparam name="TResult">The C# type of the settled value.</typeparam>
        /// <param name="first">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> MethodAsync<TResult>(Arg first, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: true, first);

        /// <summary>Declares a method that returns a promise of a value, for two arguments; the step that first uses the result waits for the promise.</summary>
        /// <typeparam name="TResult">The C# type of the settled value.</typeparam>
        /// <param name="first">The first argument: a literal, or a value read when the step runs.</param>
        /// <param name="second">The second argument.</param>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the method.</param>
        protected TypedSource<TResult> MethodAsync<TResult>(Arg first, Arg second, string? name = null, [CallerMemberName] string declaredBy = "") =>
            Result<TResult>(name, declaredBy, awaits: true, first, second);

        // ── events ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Declares an event whose listener receives a payload, declared as its own class:
        /// <c>public Event&lt;FusionGridRowSelectedArgs&gt; RowSelected =&gt; Event&lt;FusionGridRowSelectedArgs&gt;();</c>.
        /// A page handles it with <c>.Reactive(evt =&gt; evt.RowSelected, (args, p) =&gt; ..)</c>. An event
        /// declared on an object the component holds (<see cref="Object{TObject}()"/> on a property) is
        /// subscribed on that object.
        /// </summary>
        /// <typeparam name="TArgs">The declaration of the payload.</typeparam>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the event.</param>
        protected Event<TArgs> Event<TArgs>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TArgs : JsObject, new() =>
            new Event<TArgs>(EventNamed(name, declaredBy), new TArgs(), ObjectPath());

        /// <summary>
        /// Declares an event whose listener receives nothing: <c>public Event Unlocked =&gt; Event();</c>.
        /// A page handles it with <c>.Reactive(evt =&gt; evt.Unlocked, p =&gt; ..)</c>.
        /// </summary>
        /// <param name="name">The JavaScript name, when it is not the C# name camel-cased.</param>
        /// <param name="declaredBy">Supplied by the compiler: the C# member that declares the event.</param>
        protected Event Event(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new Event(EventNamed(name, declaredBy), ObjectPath());

        // ── arguments ────────────────────────────────────────────────────────────

        /// <summary>
        /// An argument the JavaScript method always takes and the page never chooses:
        /// <c>public void ShowColumn(Field key) =&gt; Method(key, Fixed("field"), name: "showColumns");</c>.
        /// </summary>
        /// <typeparam name="TValue">The C# type of the value.</typeparam>
        /// <param name="value">The value the method is always given.</param>
        protected static Arg Fixed<TValue>(TValue value) where TValue : notnull => Arg.Fixed(value);

        /// <summary>
        /// Passes a value as the wider type the JavaScript method declares, so a page's own list satisfies a
        /// method that takes any list:
        /// <c>public void UpdateData&lt;TItems&gt;(TypedSource&lt;TItems&gt; items) where TItems : IEnumerable&lt;object&gt; =&gt; Method(Pass&lt;IEnumerable&lt;object&gt;, TItems&gt;(items));</c>.
        /// </summary>
        /// <typeparam name="TDeclared">The type the JavaScript method declares.</typeparam>
        /// <typeparam name="TSource">The type of the value passed, assignable to it.</typeparam>
        /// <param name="value">The value read when the step runs.</param>
        protected static Arg Pass<TDeclared, TSource>(TypedSource<TSource> value) where TSource : TDeclared =>
            Arg.Declared<TDeclared, TSource>(value);

        // ── behind the declarations ──────────────────────────────────────────────

        private void Run(string? name, string declaredBy, bool awaits, params Arg[] args) =>
            Binding.Call(MethodNamed(name, declaredBy, awaits, args), Expressions(args));

        private TypedSource<TResult> Result<TResult>(string? name, string declaredBy, bool awaits, params Arg[] args)
        {
            var result = new MethodResult<TResult>(this, MethodNamed(name, declaredBy, awaits, args), Expressions(args));
            Binding.RunsAtThisLine(result.Expression);
            return result;
        }

        private TObject ReturnedObject<TObject>(string? name, string declaredBy, params Arg[] args) where TObject : JsObject, new()
        {
            var binding = new ReturnedObjectBinding(Binding, MethodNamed(name, declaredBy, awaits: false, args), Expressions(args));
            Binding.RunsAtThisLine(binding.Result);
            return new TObject { _binding = binding, _returned = true };
        }

        private ComponentMethod MethodNamed(string? name, string declaredBy, bool awaits, Arg[] args) =>
            ComponentMethod.Of(declaredBy, JavaScriptPath(name, declaredBy), args.Select(arg => arg.Shape).ToList(), awaits);

        private string EventNamed(string? name, string declaredBy)
        {
            if (IsReturned)
                throw new InvalidOperationException(
                    $"'{GetType().Name}.{declaredBy}' declares an event on an object a method returns. Events belong " +
                    "to the component and to the objects it holds (Object<T>() on a property).");
            return name == null ? JavaScriptNameOf(declaredBy) : OwnName(name, declaredBy);
        }

        // The path from the component's object to this object: empty for the component itself.
        private IReadOnlyList<string> ObjectPath() => _holderPath.TargetText;

        // The member's path from the object a page holds: the holder's path first for a nested object, then
        // the declared name, or the C# name camel-cased.
        private Path JavaScriptPath(string? name, string declaredBy)
        {
            if (name == null) return _holderPath.Then(JavaScriptNameOf(declaredBy));
            // name: "" declares the object itself: a plugin that is a function.
            return name.Length == 0 ? _holderPath : _holderPath.Then(OwnName(name, declaredBy));
        }

        // A declared name is one JavaScript name. A member of an object the component holds is declared on
        // that object (Object<T>()), and a field of each item of a list is read with Each(..): one way to
        // reach either, which the compiler checks.
        private string OwnName(string name, string declaredBy)
        {
            var reachesIntoAnObject = name.IndexOf('.') >= 0 || name.IndexOf('[') >= 0;
            return reachesIntoAnObject
                ? throw new ArgumentException(
                    $"'{GetType().Name}.{declaredBy}' is named '{name}', a path into an object or a list it holds. " +
                    "Declare the object with Object<T>() and the member on it, or read a field of each item " +
                    "with the list's Each(item => item.Field).", nameof(name))
                : name;
        }

        // The kind of C# member decides: a property holds an object, a method returns one. A member that
        // implements an interface explicitly carries the interface in its name (IHeadedBoard.Header).
        private bool DeclaresMethod(string declaredBy)
        {
            var members = GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(member => member.Name == declaredBy || member.Name.EndsWith("." + declaredBy, StringComparison.Ordinal))
                .ToArray();
            if (members.Length == 0)
                throw new ArgumentException(
                    $"'{GetType().Name}' has no member '{declaredBy}'. Declare an object as a C# property or method, " +
                    "for example public Settings Settings => Object<Settings>(); or public Reading Reading() => Object<Reading>();",
                    nameof(declaredBy));
            return members.All(member => member is MethodInfo);
        }

        private static IReadOnlyList<ValueExpression> Expressions(Arg[] args) =>
            args.Select(arg => arg.Expression).ToList();

        // A member is named by its C# name, camel-cased, unless the declaration names it.
        private static string JavaScriptNameOf(string declaredBy)
        {
            if (string.IsNullOrEmpty(declaredBy))
                throw new ArgumentException("Declare a member as a C# property or method, for example public void Show() => Method();", nameof(declaredBy));

            return JsonMemberName.Of(declaredBy);
        }

        // A method's result: the value a line reads, bound once in the callback that uses it.
        private sealed class MethodResult<TResult> : TypedSource<TResult>
        {
            internal MethodResult(JsObject owner, ComponentMethod method, IReadOnlyList<ValueExpression> args)
            {
                Expression = owner.Binding.Invoke(method, args, Shape);
            }

            internal override ValueExpression Expression { get; }
        }

        // The object a method returned a list in: its items are reached through the result.
        private sealed class ReturnedList : JsObject
        {
        }
    }
}
