using System;
using System.IO;
using System.Linq.Expressions;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Framework-agnostic base for a model-bound input field.
    /// </summary>
    /// <remarks>
    /// Captures the model expression, plan, and field options. <typeparamref name="THelper"/>
    /// is left open here and closed by the platform-specific subclass (e.g.
    /// <c>InputBoundField&lt;TModel, TProp&gt;</c> closes it to <c>IHtmlHelper</c>
    /// for ASP.NET Core).
    /// </remarks>
    /// <typeparam name="THelper">The platform-specific HTML helper type.</typeparam>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The model property type the field is bound to.</typeparam>
    public class InputBoundFieldBase<THelper, TModel, TProp> where TModel : class
    {
        /// <summary>Gets the platform-specific HTML helper for rendering.</summary>
        public THelper Helper { get; }

        /// <summary>Gets the plan this field belongs to.</summary>
        public ReactivePlan<TModel> Plan { get; }

        /// <summary>Gets the model property expression this field is bound to.</summary>
        public Expression<Func<TModel, TProp>> Expression { get; }

        /// <summary>Gets the label and required options for this field.</summary>
        public InputFieldOptions Options { get; }

        /// <summary>Gets the generated HTML element ID for this field's input.</summary>
        internal string ElementId { get; }

        /// <summary>Gets the model binding path (e.g. <c>"Address.City"</c>) for validation message targeting.</summary>
        internal string BindingPath { get; }

        /// <summary>Gets the writer for emitting HTML output.</summary>
        internal TextWriter Writer { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by platform-specific factories
        /// like <c>Html.InputField()</c>. Public constructors would bypass the ID generation
        /// and component registration that field wrappers depend on.
        /// </summary>
        internal InputBoundFieldBase(
            THelper helper,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            string elementId,
            string bindingPath,
            TextWriter writer)
        {
            Helper = helper;
            Plan = plan;
            Expression = expression;
            Options = options;
            ElementId = elementId;
            BindingPath = bindingPath;
            Writer = writer;
        }

        /// <summary>
        /// Renders the field wrapper (label + validation error elements) around content
        /// written by the callback. Throws if the component was not registered via
        /// <c>AddToComponentsMap</c>: unregistered components are invisible to validation
        /// and gather, causing silent failures.
        /// </summary>
        internal void Render(Action writeContent)
        {
            if (!Plan.ComponentsMap.ContainsKey(BindingPath))
                throw new InvalidOperationException(
                    $"Component for '{BindingPath}' was rendered without calling " +
                    $"plan.AddToComponentsMap(). Validation and gather will not work. " +
                    $"Add plan.AddToComponentsMap(\"{BindingPath}\", ...) in your HtmlExtensions factory.");

            var fb = new InputFieldBuilder(Writer, BindingPath).ForId(ElementId);
            if (Options.LabelText != null) fb.Label(Options.LabelText);
            if (Options.IsRequired) fb.Required();
            using (fb.Begin()) { writeContent(); }
        }
    }
}
