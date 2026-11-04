using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Components;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Document;
using Alis.Reactive.PlanModel.Validation;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as generated Reactive Plan JSON.
    /// </summary>
    /// <typeparam name="TModel">The view's model, whose members name the plan's inputs.</typeparam>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private readonly RenderedInputs _renderedInputs = new RenderedInputs();

        private readonly PlanBuildContext _context;
        private readonly IServiceProvider? _services;

        internal ReactivePlan(IServiceProvider? services, string fieldPrefix, string view)
        {
            _services = services;
            ClassNames = ResolveClassNames(services);
            _context = new PlanBuildContext(PlanId.ForModel(typeof(TModel)).Value, _renderedInputs, fieldPrefix, view, ClassNames);
        }

        internal IReadOnlyList<string> Forms => _context.Forms;
        internal PlanBuildContext Context => _context;

        internal ReactiveClassNames ClassNames { get; }

        /// <summary>Registers a plugin with the plan; the returned plugin's members are used directly.</summary>
        /// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
        /// <returns>The plugin: <c>slugify.Slug(planner.Wing)</c>.</returns>
        public TPlugin RegisterPlugin<TPlugin>()
            where TPlugin : Plugin, new()
        {
            var plugin = new TPlugin();
            _context.RegisterPlugin(plugin.Name);
            plugin.BindTo(new ModuleBinding(Target.Plugin(plugin.Name), _context));
            return plugin;
        }

        /// <summary>
        /// Joins the markup a component helper rendered for <paramref name="elementId"/> to this plan,
        /// so behavior can hang off it with <c>.Reactive(...)</c>; the view writes the returned
        /// component where the markup belongs. Inputs render into their field instead:
        /// <c>field.Render&lt;TComponent&gt;(markup)</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var builder = html.EJS().Dialog(elementId);
        /// build(builder);
        /// return plan.Render&lt;FusionDialog&gt;(elementId, builder.Render());
        /// </code>
        /// </example>
        /// <typeparam name="TComponent">The component declaration the markup renders.</typeparam>
        /// <param name="elementId">The ID the markup gives the component's element.</param>
        /// <param name="markup">The component's markup.</param>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(
            string elementId,
#if NET48
            System.Web.IHtmlString markup)
#else
            Microsoft.AspNetCore.Html.IHtmlContent markup)
#endif
            where TComponent : IComponent, new()
        {
            if (string.IsNullOrWhiteSpace(elementId))
                throw new ArgumentException("Element ID required.", nameof(elementId));
            if (markup == null) throw new ArgumentNullException(nameof(markup));
            return new RenderedComponent<TComponent, TModel>(this, elementId, markup);
        }

        internal void AddRenderedInput(RenderedInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _context.AddInput(input);
        }

        /// <summary>
        /// The browser rules <typeparamref name="TValidator"/> declares, resolved from the services the plan
        /// renders with, for a component whose own JavaScript validates: a grid's cell editor.
        /// </summary>
        /// <example><c>FusionGridValidation.From&lt;ResidentCareItem&gt;(plan.BrowserRules&lt;ResidentCareItemValidator&gt;())</c></example>
        /// <typeparam name="TValidator">The validator registered with <c>AddReactiveFluentValidation</c>.</typeparam>
        /// <returns>Each field the validator names, with the rules the browser can evaluate.</returns>
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>() where TValidator : class
            => RequireBrowserRuleSource(_services, CSharpTypeName.Of(typeof(TValidator)), "This view")
                .GetBrowserRules<TValidator>();

        /// <summary>The browser rules <typeparamref name="TValidator"/> declares in one set (<c>RuleSet("Issue", ..)</c>).</summary>
        /// <typeparam name="TValidator">The validator registered with <c>AddReactiveFluentValidation</c>.</typeparam>
        /// <param name="ruleSet">The set, as the validator's <c>RuleSet</c> named it; a set it does not declare is refused.</param>
        /// <returns>Each field the set names, with the rules the browser can evaluate.</returns>
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>(string ruleSet) where TValidator : class
            => RequireBrowserRuleSource(_services, CSharpTypeName.Of(typeof(TValidator)), "This view")
                .GetBrowserRules<TValidator>(ruleSet);

        internal string Serialize()
        {
            _context.Seal();
            ResolveAll();
            return GrammarSerializer.Serialize(_context.Write());
        }

        private void ResolveAll()
        {
            _context.DeclareRenderedInputs();

            var oneByForm = _context.Validations
                .GroupBy(validation => validation.Form, StringComparer.Ordinal)
                .Select(sameForm => sameForm.First())
                .ToList();
            if (oneByForm.Count > 0)
            {
                var first = oneByForm[0];
                var ruleSource = RequireBrowserRuleSource(_services, CSharpTypeName.Of(first.Validator), $"Form '{first.Form}'");
                foreach (var validation in oneByForm)
                {
                    var fields = validation.Rules(ruleSource);
                    _context.Check(validation.Form, CheckedField.Bind(validation.Form, fields, _context.FieldPrefix).ToList());
                }
            }
        }

        private static ReactiveClassNames ResolveClassNames(IServiceProvider? services)
        {
            var names = services?.GetService(typeof(ReactiveClassNames)) as ReactiveClassNames;

#if NET48
            names ??= System.Web.Mvc.DependencyResolver.Current?.GetService(typeof(ReactiveClassNames)) as ReactiveClassNames;
#endif

            return names ?? new ReactiveClassNames();
        }

        private static IBrowserRuleSource RequireBrowserRuleSource(
            IServiceProvider? services,
            string validatorName,
            string requiredBy)
        {
            var source = services?.GetService(typeof(IBrowserRuleSource)) as IBrowserRuleSource;

#if NET48
            source ??= System.Web.Mvc.DependencyResolver.Current?.GetService(typeof(IBrowserRuleSource))
                as IBrowserRuleSource;
#endif

            if (source != null) return source;

#if NET48
            throw new InvalidOperationException(
                $"{requiredBy} specifies validation source '{validatorName}', " +
                "but no IBrowserRuleSource could be resolved. " +
                "Register the validators with AddReactiveFluentValidation(...) and bridge it to " +
                "MVC5 by calling DependencyResolver.SetResolver(...) in Application_Start.");
#else
            throw new InvalidOperationException(
                $"{requiredBy} specifies validation source '{validatorName}', " +
                "but no IBrowserRuleSource is registered in DI. " +
                "Register the validators with services.AddReactiveFluentValidation(...), " +
                "or register your own IBrowserRuleSource.");
#endif
        }
    }
}
