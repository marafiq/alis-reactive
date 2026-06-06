using System;
using System.Linq.Expressions;
using Alis.Reactive.InputField;
#if NET48
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Extensions
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
        /// Field wrapper owns label display and validation error placement.
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
            // System.Web.Mvc NameFor honors HtmlFieldPrefix and returns MvcHtmlString; the slot stores a string binding path.
            var bindingName = html.NameFor(expression).ToHtmlString();
#else
            var bindingName = html.NameFor(expression);
#endif
            var componentSlot = ModelBoundInputComponentSlot.For<TModel, TProp>(
                expression,
                bindingName);
            var boundField = BoundInputField<TModel, TProp>.Create(
                plan,
                expression,
                options,
                componentSlot);

            return new InputBoundField<TModel, TProp>(
                html,
                boundField,
                html.ViewContext.Writer);
        }
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
