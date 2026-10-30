using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

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

        // The plan id names the plan in traces only: the model's type, which many plans of a page may share.
        internal ReactivePlan(IServiceProvider? services, string fieldPrefix, string view)
        {
            _services = services;
            _context = new PlanBuildContext(PlanId.ForModel(typeof(TModel)).Value, _renderedInputs, fieldPrefix, view);
        }

        /// <summary>The forms this plan validates or shows the server's errors on, by container id.</summary>
        internal IReadOnlyList<string> Forms => _context.Forms;
        internal PlanBuildContext Context => _context;

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
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>() where TValidator : class =>
            RequireBrowserRuleSource(_services, CSharpTypeName.Of(typeof(TValidator)), "This view")
                .GetBrowserRules<TValidator>();

        /// <summary>
        /// Registers components, resolves validation, and serializes the Reactive Plan as compact JSON. A plan renders
        /// once, after everything written to it: an input or behavior written to it later, and a second rendering, are
        /// refused.
        /// </summary>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render()
        {
            _context.Seal();
            ResolveAll(_services);
            return GrammarSerializer.Serialize(_context.Write());
        }

        /// <summary>
        /// Registers components, resolves validation with the supplied services, and serializes the Reactive Plan as
        /// compact JSON. A plan renders once, after everything written to it, as <see cref="Render()"/> does.
        /// </summary>
        /// <param name="services">Services used to resolve validation rule sources required by queued validation rules.</param>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render(IServiceProvider services)
        {
            _context.Seal();
            ResolveAll(services);
            return GrammarSerializer.Serialize(_context.Write());
        }

        // Every rendered input joins the plan, then each validated form is bound to its one validator's rules:
        // every request that validates a form names the same validator, so the form is bound once. A form only the
        // server checks has no fields: its messages land beside the mounted inputs inside it (grammar.md F8).
        private void ResolveAll(IServiceProvider? services)
        {
            _context.DeclareRenderedInputs();
            var renderedInputs = _renderedInputs.Snapshot();

            var validated = _context.ValidatedRequests;
            var oneByForm = validated
                .Select(request => request.Validation)
                .GroupBy(validation => validation.Form, StringComparer.Ordinal)
                .Select(sameForm => sameForm.First())
                .ToList();
            if (oneByForm.Count > 0)
            {
                var ruleSource = RequireBrowserRuleSource(
                    services, CSharpTypeName.Of(validated[0].Validation.Validator), $"Request at '{validated[0].Request.Url}'");
                foreach (var validation in oneByForm)
                {
                    var fields = validation.Rules(ruleSource);
                    _context.Check(validation.Form, CheckedField.Bind(fields, renderedInputs, typeof(TModel), _context.FieldPrefix).ToList());
                }
            }
        }

        private static IBrowserRuleSource RequireBrowserRuleSource(
            IServiceProvider? services,
            string validatorName,
            string requiredBy)
        {
            // ASP.NET Core flows the source through the per-request IServiceProvider.
            var source = services?.GetService(typeof(IBrowserRuleSource)) as IBrowserRuleSource;

#if NET48
            // net48 / System.Web has no per-request IServiceProvider. MVC5's idiomatic
            // service locator is DependencyResolver, bridged at startup via SetResolver.
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
