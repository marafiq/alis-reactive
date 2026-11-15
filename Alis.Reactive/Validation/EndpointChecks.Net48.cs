#if NET48
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Alis.Reactive.Tracing;
using Alis.Reactive.TypeKnowledge;

using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Validation;

/// <summary>
/// The check an endpoint runs on the model its action bound.
/// </summary>
public static class EndpointChecks
{
    /// <summary>Checks the model the action bound by the validator named, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The validator is the one the app registered. Return the result's <c>ToErrorResponse()</c> when it is not valid.
    /// The app supplies its registered validator through MVC's dependency resolver.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it, the one the request names in the view.</typeparam>
    /// <returns>The check's result.</returns>
    public static ValidationResult Validate<TRequest, TValidator>(this Controller controller, TRequest request)
        where TValidator : ReactiveValidator<TRequest>
        => Validate<TRequest, TValidator>(controller, request, "default");

    /// <summary>Checks the model the action bound by the validator named, with its asynchronous rules, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The validator is the one the app registered. Return the result's <c>ToErrorResponse()</c> when it is not valid.
    /// The app supplies its registered validator through MVC's dependency resolver.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <param name="cancellation">Cancels the check.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it, the one the request names in the view.</typeparam>
    /// <returns>The check's result.</returns>
    public static Task<ValidationResult> ValidateAsync<TRequest, TValidator>(this Controller controller, TRequest request, CancellationToken cancellation = default)
        where TValidator : ReactiveValidator<TRequest>
        => ValidateAsync<TRequest, TValidator>(controller, request, "default", cancellation);

    /// <summary>Checks the model the action bound by the validator and the rule set named, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The check runs the rules the validator declares inside that rule set and no other. Return the result's
    /// <c>ToErrorResponse()</c> when it is not valid.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <param name="ruleSet">The rule set's name, as the request names it in the view.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it.</typeparam>
    /// <returns>The check's result.</returns>
    public static ValidationResult Validate<TRequest, TValidator>(this Controller controller, TRequest request, string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
    {
        var services = new ResolverServices();
        var catalog = services.GetService<ValidatorCatalog>() ?? throw ValidatorCatalog.Missing<TRequest, TValidator>();
        var validator = catalog.Resolve<TRequest, TValidator>(services);
        using var check = ServerActivities.Check(ClassNames.AsCSharpWritesIt(typeof(TValidator)), ruleSet);
        var result = validator.CheckBound(request, ruleSet, BindingFailures(controller), catalog.Flags);
        ServerActivities.Checked(check, result.Errors.Count);
        return result;
    }

    /// <summary>Checks the model the action bound by the validator and the rule set named, with its asynchronous rules, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The check runs the rules the validator declares inside that rule set and no other. Return the result's
    /// <c>ToErrorResponse()</c> when it is not valid.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <param name="ruleSet">The rule set's name, as the request names it in the view.</param>
    /// <param name="cancellation">Cancels the check.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it.</typeparam>
    /// <returns>The check's result.</returns>
    public static async Task<ValidationResult> ValidateAsync<TRequest, TValidator>(this Controller controller, TRequest request, string ruleSet, CancellationToken cancellation = default)
        where TValidator : ReactiveValidator<TRequest>
    {
        var services = new ResolverServices();
        var catalog = services.GetService<ValidatorCatalog>() ?? throw ValidatorCatalog.Missing<TRequest, TValidator>();
        var validator = catalog.Resolve<TRequest, TValidator>(services);
        using var check = ServerActivities.Check(ClassNames.AsCSharpWritesIt(typeof(TValidator)), ruleSet);
        var result = await validator.CheckBoundAsync(request, ruleSet, BindingFailures(controller), catalog.Flags, cancellation).ConfigureAwait(false);
        ServerActivities.Checked(check, result.Errors.Count);
        return result;
    }

    private static IReadOnlyList<ValidationFailure> BindingFailures(Controller controller)
        => controller.ModelState.SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationFailure(entry.Key,
            error.ErrorMessage.Length > 0 ? error.ErrorMessage : string.Format(CultureInfo.CurrentCulture,
                "The value '{0}' is not valid for {1}.", entry.Value.Value?.AttemptedValue, entry.Key.Substring(entry.Key.LastIndexOf('.') + 1))))).ToArray();

    private sealed class ResolverServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => DependencyResolver.Current.GetService(serviceType);
    }
}
#endif
