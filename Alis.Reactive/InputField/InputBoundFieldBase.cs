using System;
using System.IO;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

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
    /// <typeparam name="THelper">The platform-specific HTML helper type.</typeparam>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model property type the field is bound to.</typeparam>
    public class InputBoundFieldBase<THelper, TModel, TProp> where TModel : class
    {
        /// <summary>Gets the platform-specific HTML helper for rendering.</summary>
        public THelper Helper { get; }

        /// <summary>Gets the Reactive Plan that owns this field registration.</summary>
        public ReactivePlan<TModel> Plan { get; }

        /// <summary>Gets the model property expression this field is bound to.</summary>
        public Expression<Func<TModel, TProp>> Expression { get; }

        /// <summary>Gets the label and required options for this field.</summary>
        public InputFieldOptions Options { get; }

        /// <summary>Gets the generated HTML element ID for this field's input.</summary>
        internal string ElementId => _componentSlot.ElementId;

        /// <summary>Gets the model binding path (e.g. <c>"Address.City"</c>) for validation message targeting.</summary>
        internal string BindingPath => _componentSlot.BindingName;

        /// <summary>Gets the controlled DOM/render target owned by the model-bound component slot.</summary>
        internal InputComponentRenderTarget RenderTarget => _componentSlot.RenderTarget;

        /// <summary>Gets the writer for emitting HTML output.</summary>
        internal TextWriter Writer { get; }

        private readonly ModelBoundInputComponentSlot _componentSlot;

        internal void RegisterInputComponent(InputComponentRegistrationProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            Plan.RegisterInputComponent(_componentSlot.Register(profile));
        }

        /// <summary>
        /// Keep internal: platform-specific factories such as <c>Html.InputField()</c>
        /// create the controlled component id and registration slot that field wrappers depend on.
        /// </summary>
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
            _componentSlot = field.ComponentSlot;
            Writer = writer;
        }

        /// <summary>
        /// Renders the field wrapper (label + validation error elements) around content
        /// written by the callback. Throws if the component was not registered via
        /// <c>RegisterInputComponent</c>: unregistered components are invisible to validation
        /// and gather, causing silent failures.
        /// </summary>
        internal void Render(Action writeContent)
        {
            if (!Plan.HasRegisteredInputComponent(_componentSlot.BindingPath))
                throw new InvalidOperationException(
                    $"Input field '{BindingPath}' rendered without registering an input component. " +
                    "Validation and gather depend on the registered input contract. " +
                    "Call setup.RegisterInputComponent(...) in the component HtmlExtension before rendering the component.");

            var fb = new InputFieldBuilder(Writer, BindingPath).ForId(ElementId);
            if (Options.LabelText != null) fb.Label(Options.LabelText);
            if (Options.IsRequired) fb.Required();
            using (fb.Begin()) { writeContent(); }
        }
    }

    internal sealed class BoundInputField<TModel, TProp> where TModel : class
    {
        private BoundInputField(
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            ModelBoundInputComponentSlot componentSlot)
        {
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ComponentSlot = componentSlot ?? throw new ArgumentNullException(nameof(componentSlot));
        }

        internal ReactivePlan<TModel> Plan { get; }
        internal Expression<Func<TModel, TProp>> Expression { get; }
        internal InputFieldOptions Options { get; }
        internal ModelBoundInputComponentSlot ComponentSlot { get; }

        internal static BoundInputField<TModel, TProp> Create(
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            ModelBoundInputComponentSlot componentSlot) =>
            new BoundInputField<TModel, TProp>(plan, expression, options, componentSlot);
    }
}
