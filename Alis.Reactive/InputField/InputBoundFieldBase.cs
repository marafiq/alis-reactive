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
        public THelper Helper { get; }
        public ReactivePlan<TModel> Plan { get; }
        public Expression<Func<TModel, TProp>> Expression { get; }
        public InputFieldOptions Options { get; }

        internal string ElementId { get; }
        internal string BindingPath { get; }
        internal TextWriter Writer { get; }

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
        /// Renders the field wrapper (label + validation error HTML elements) around content
        /// written by the callback. Throws if the component was not registered via
        /// <c>AddToComponentsMap</c> — unregistered components are invisible to validation
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
