using System;
using System.Linq.Expressions;
using Alis.Reactive.InputField;
#if NET48
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// Razor view extension for starting a model-bound input field.
    /// </summary>
    public static class InputFieldExtensions
    {
        /// <summary>
        /// Starts a model-bound input field for <paramref name="expression"/>, with optional
        /// label and required marker.
        /// </summary>
        /// <remarks>
        /// Chain a component extension on the result to choose the rendered control.
        /// Field wrapper owns label display and validation error placement. A field renders its element id
        /// once on a page: rendered again, by this plan or another, it is refused; so is a field the view's
        /// field prefix changed under after the plan started.
        /// </remarks>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model property type the field is bound to.</typeparam>
        /// <param name="plan">Reactive Plan that owns this field registration.</param>
        /// <param name="expression">Model property to bind the field to.</param>
        /// <returns>Bound field ready to receive a component extension.</returns>
        public static InputBoundField<TModel, TProp> InputField<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
            => CreateInputField(html, plan, expression, InputFieldConfiguration.Default);

        /// <summary>
        /// Starts a model-bound input field for <paramref name="expression"/>, with label
        /// and required marker configuration.
        /// </summary>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model property type the field is bound to.</typeparam>
        /// <param name="plan">Reactive Plan that owns this field registration.</param>
        /// <param name="expression">Model property to bind the field to.</param>
        /// <param name="configure">Configures label text and required marker.</param>
        /// <returns>Bound field ready to receive a component extension.</returns>
        public static InputBoundField<TModel, TProp> InputField<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            Action<InputFieldOptions> configure)
            where TModel : class
            => CreateInputField(html, plan, expression, InputFieldConfiguration.Configured(configure));

        private static InputBoundField<TModel, TProp> CreateInputField<TModel, TProp>(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldConfiguration configuration)
            where TModel : class
        {
            var options = configuration.CreateOptions();
#if NET48
            // System.Web.Mvc NameFor honors HtmlFieldPrefix and returns MvcHtmlString; the identity keeps the binding path as text.
            var bindingName = html.NameFor(expression).ToHtmlString();
#else
            var bindingName = html.NameFor(expression);
#endif
            RefuseAPrefixTheViewTookAfterThePlanStarted(html.ViewData.TemplateInfo.HtmlFieldPrefix, plan.Context, bindingName);
            var identity = InputIdentity.For<TModel, TProp>(expression, bindingName, plan.Context.FieldPrefix);
            RenderedPage.Of(html.ViewContext).Input(identity.ElementId, identity.BindingName, RenderedPage.ViewPathOf(html.ViewContext));
            var boundField = BoundInputField<TModel, TProp>.Create(
                plan,
                expression,
                options,
                identity);

            return new InputBoundField<TModel, TProp>(
                html,
                boundField,
                html.ViewContext.Writer);
        }

        // MVC names the input under the prefix the view renders under now, and the plan sends and checks it under the
        // prefix the view rendered under when the plan started: they are one prefix, or the input posts under a name
        // its plan does not know.
        private static void RefuseAPrefixTheViewTookAfterThePlanStarted(string prefix, PlanModel.PlanBuildContext plan, string bindingName)
        {
            if (prefix == plan.FieldPrefix) return;
            throw new InvalidOperationException(
                $"The field prefix of {plan.View} changed from {Described(plan.FieldPrefix)} to {Described(prefix)} after its " +
                $"plan started, so input '{bindingName}' would post under a name its plan does not send or check. On .NET " +
                "Framework a ViewDataDictionary copied from ViewData shares the view's TemplateInfo, so giving the copy a " +
                "prefix gives it to the view: render the member with Html.EditorFor, or give the copy a new TemplateInfo " +
                $"{{ HtmlFieldPrefix = \"{prefix}\" }}.");
        }

        private static string Described(string prefix) => prefix.Length == 0 ? "none" : $"'{prefix}'";
    }

    internal abstract class InputFieldConfiguration
    {
        private protected InputFieldConfiguration() { }

        internal static InputFieldConfiguration Default { get; } =
            new DefaultInputFieldConfiguration();

        internal static InputFieldConfiguration Configured(Action<InputFieldOptions> configure) =>
            new ConfiguredInputFieldConfiguration(configure);

        internal abstract InputFieldOptions CreateOptions();
    }

    internal sealed class DefaultInputFieldConfiguration : InputFieldConfiguration
    {
        internal override InputFieldOptions CreateOptions() => new InputFieldOptions();
    }

    internal sealed class ConfiguredInputFieldConfiguration : InputFieldConfiguration
    {
        private readonly Action<InputFieldOptions> _configure;

        internal ConfiguredInputFieldConfiguration(Action<InputFieldOptions> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        internal override InputFieldOptions CreateOptions()
        {
            var options = new InputFieldOptions();
            _configure(options);
            return options;
        }
    }
}
