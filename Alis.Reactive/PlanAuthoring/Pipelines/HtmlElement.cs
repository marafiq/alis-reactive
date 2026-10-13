using System;
using Alis.Reactive.Builders.Arrays;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// An element of the page, found by the id the view gave it: <c>p.Element("status")</c>. Its text,
    /// its HTML, its class list and whether it shows.
    /// </summary>
    /// <example>
    /// <code>
    /// p.Element("status").Text.Set("Saved");
    /// s.Element("resident-name").Text.Set(json.Get(x =&gt; x.Name));
    /// p.Element("banner").ClassList.Add("is-active");
    /// p.Element("details").Show();
    /// </code>
    /// </example>
    public sealed class HtmlElement
    {
        private readonly PlanBuildContext _context;
        private readonly string _id;

        internal HtmlElement(PlanBuildContext context, string id)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _id = id ?? throw new ArgumentNullException(nameof(id));
            context.DeclareElement(id);
            Text = new ElementContent(this, BrowserElementMembers.Text);
            Html = new ElementContent(this, BrowserElementMembers.Html);
            ClassList = new ElementClassList(this);
        }

        /// <summary>The element's text (<c>textContent</c>).</summary>
        public ElementContent Text { get; }

        /// <summary>The element's HTML (<c>innerHTML</c>).</summary>
        public ElementContent Html { get; }

        /// <summary>The element's classes (<c>classList</c>).</summary>
        public ElementClassList ClassList { get; }

        /// <summary>The element's child elements (<c>children</c>), for counting and testing:
        /// <c>p.Element("risk-list").Children.Count(x =&gt; x.GetAttribute("data-risk") == "high")</c>.</summary>
        public ReactiveArray<ChildElement> Children =>
            new ReactiveArray<ChildElement>(Read("children"), Shape.FromClrType(typeof(ChildElement)));

        /// <summary>Shows the element: removes its <c>hidden</c> attribute.</summary>
        public void Show() => Set(BrowserElementMembers.Hidden, ValueExpression.Literal(false));

        /// <summary>Hides the element: sets its <c>hidden</c> attribute.</summary>
        public void Hide() => Set(BrowserElementMembers.Hidden, ValueExpression.Literal(true));

        internal ValueExpression Read(string member) => ValueExpression.Read(Receiver.Element(_id), new[] { member }, Shape.None);

        internal void Set<TValue>(ComponentProperty<TValue> property, ValueExpression value) =>
            _context.AddStep(new SetStep(Receiver.Element(_id), G.PathOf(property.Path), property.Shape, value));

        internal void Call(ComponentMethod method, ValueExpression argument) =>
            _context.AddStep(new CallStep(Receiver.Element(_id), G.PathOf(method.Path), new[] { argument }, method.ArgumentShapes, method.Awaits));
    }

    /// <summary>An element's text or HTML: set to a literal, or to a value read when the step runs, shown as it arrives.</summary>
    public sealed class ElementContent
    {
        private readonly HtmlElement _element;
        private readonly ComponentProperty<string> _property;

        internal ElementContent(HtmlElement element, ComponentProperty<string> property)
        {
            _element = element;
            _property = property;
        }

        /// <summary>Sets it to text fixed when the view renders.</summary>
        /// <param name="value">The text.</param>
        public void Set(string value) => _element.Set(_property, ValueExpression.Literal(value));

        /// <summary>Sets it to a value read when the step runs: a number, a date or any text, shown as it arrives.</summary>
        /// <typeparam name="TValue">The value's type.</typeparam>
        /// <param name="source">The value.</param>
        public void Set<TValue>(TypedSource<TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _element.Set(_property, source.Expression.AsArrived());
        }
    }

    /// <summary>An element's classes.</summary>
    public sealed class ElementClassList
    {
        private readonly HtmlElement _element;

        internal ElementClassList(HtmlElement element)
        {
            _element = element;
        }

        /// <summary>The classes the element has, for counting and testing.</summary>
        public ReactiveArray<string> Items =>
            new ReactiveArray<string>(_element.Read("classList"), Shape.String);

        /// <summary>Adds a class.</summary>
        /// <param name="name">The class, or a value read when the step runs.</param>
        public void Add(Arg<string> name) => _element.Call(BrowserElementMembers.AddClass, Of(name));

        /// <summary>Removes a class.</summary>
        /// <param name="name">The class, or a value read when the step runs.</param>
        public void Remove(Arg<string> name) => _element.Call(BrowserElementMembers.RemoveClass, Of(name));

        /// <summary>Adds the class when the element lacks it, removes it when it has it.</summary>
        /// <param name="name">The class, or a value read when the step runs.</param>
        public void Toggle(Arg<string> name) => _element.Call(BrowserElementMembers.ToggleClass, Of(name));

        private static ValueExpression Of(Arg<string> name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return name.Expression;
        }
    }

    /// <summary>
    /// A child element, read inside an array predicate or projection over <see cref="HtmlElement.Children"/>.
    /// Its members are read in the browser; C# only checks the expression that names them.
    /// </summary>
    public sealed class ChildElement
    {
        private ChildElement()
        {
        }

        /// <summary>The value of the child's attribute, read in the browser: <c>x =&gt; x.GetAttribute("data-risk")</c>.</summary>
        /// <param name="name">The attribute's name.</param>
        public string? GetAttribute(string name) =>
            throw new InvalidOperationException("A child element is read in the browser, inside an array predicate or projection.");
    }
}
