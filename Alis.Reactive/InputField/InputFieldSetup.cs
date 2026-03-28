using System;
using System.IO;
using System.Linq.Expressions;

namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Captures expression, plan, and options for a model-bound input field.
    /// <typeparamref name="THelper"/> is the framework-specific HTML helper —
    /// left open here, closed at the app level (e.g. IHtmlHelper for ASP.NET Core).
    /// </summary>
    public class InputFieldSetup<THelper, TModel, TProp> where TModel : class
    {
        public THelper Helper { get; }
        public ReactivePlan<TModel> Plan { get; }
        public Expression<Func<TModel, TProp>> Expression { get; }
        public InputFieldOptions Options { get; }

        internal string ElementId { get; }
        internal string BindingPath { get; }
        internal TextWriter Writer { get; }

        internal InputFieldSetup(
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
        /// Renders the field wrapper (label + validation slot) around content written by the callback.
        /// Throws if the component was not registered via AddToComponentsMap — unregistered components
        /// are invisible to validation and gather, causing silent failures.
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
