using System;
using System.IO;
using System.Linq.Expressions;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Framework-agnostic base for a model-bound input field.
    /// </summary>
    /// <remarks>
    /// Captures the model expression, plan, and field options. Application code uses the
    /// platform-specific subclass (<c>InputBoundField&lt;TModel, TProp&gt;</c> for ASP.NET Core)
    /// returned by <c>Html.InputField()</c>.
    /// </remarks>
    /// <typeparam name="THelper">Platform-specific HTML helper type.</typeparam>
    /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">Model property type the field is bound to.</typeparam>
    public class InputBoundFieldBase<THelper, TModel, TProp> where TModel : class
    {
        /// <summary>HTML helper used by component render factories.</summary>
        public THelper Helper { get; }

        /// <summary>The plan the rendered input joins.</summary>
        public ReactivePlan<TModel> Plan { get; }

        /// <summary>Model expression used to derive the field binding path and component ID.</summary>
        public Expression<Func<TModel, TProp>> Expression { get; }

        /// <summary>Field wrapper options used for label and required markup.</summary>
        public InputFieldOptions Options { get; }

        /// <summary>The element ID the framework derives from the model expression. Render the component with it.</summary>
        public string ElementId => _identity.ElementId;

        /// <summary>The binding path; render it as the component's <c>name</c> so the value posts back.</summary>
        public string BindingPath => _identity.BindingName;

        internal TextWriter Writer { get; }

        private readonly InputIdentity _identity;

        // The plan learns which component holds the input's value and which event commits it.
        internal void AddRenderedInput(IInputComponent component) =>
            Plan.AddRenderedInput(RenderedInput.Of(_identity, component));

        // Internal because Html.InputField decides the input's identity, which validation and gather read.
        internal InputBoundFieldBase(
            THelper helper,
            BoundInputField<TModel, TProp> field,
            TextWriter writer)
        {
            Helper = helper;
            if (field == null) throw new ArgumentNullException(nameof(field));
            Plan = field.Plan;
            Expression = field.Expression;
            Options = field.Options;
            _identity = field.Identity;
            Writer = writer;
        }

        /// <summary>
        /// Renders the field wrapper (label + validation error elements) around content
        /// written by the callback, or the content alone for a hidden field. Throws when no component
        /// rendered the input: an input the plan does not know is invisible to validation and gather.
        /// </summary>
        internal void Render(Action writeContent)
        {
            if (!Plan.HasRenderedInput(_identity.Binding))
                throw new InvalidOperationException(
                    $"Input field '{BindingPath}' was rendered without its component. " +
                    "Render an input through field.Render<TComponent>(markup), so the plan knows which component holds its value.");

            if (Options.IsHidden)
            {
                writeContent();
                return;
            }

            var fieldMarkup = new InputFieldBuilder(Writer, BindingPath).ForInputId(ElementId);
            if (Options.LabelText != null) fieldMarkup.Label(Options.LabelText);
            if (Options.IsRequired) fieldMarkup.Required();
            using (fieldMarkup.Begin()) { writeContent(); }
        }
    }

    internal sealed class BoundInputField<TModel, TProp> where TModel : class
    {
        private BoundInputField(
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            InputIdentity identity)
        {
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        internal ReactivePlan<TModel> Plan { get; }
        internal Expression<Func<TModel, TProp>> Expression { get; }
        internal InputFieldOptions Options { get; }
        internal InputIdentity Identity { get; }

        internal static BoundInputField<TModel, TProp> Create(
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            InputIdentity identity) =>
            new BoundInputField<TModel, TProp>(plan, expression, options, identity);
    }
}
