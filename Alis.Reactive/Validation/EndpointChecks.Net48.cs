#if NET48
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Alis.Reactive.Validation;

/// <summary>
/// The check an endpoint runs on the model its action bound.
/// </summary>
public static class EndpointChecks
{
    /// <summary>Checks the model the action bound by the validator named, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The validator is the one the app registered. Return the result's <c>ToErrorResponse()</c> when it is not valid.
    /// MVC's own validation adds nothing to the result.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it, the one the request names in the view.</typeparam>
    /// <returns>The check's result.</returns>
    public static ValidationResult Validate<TRequest, TValidator>(this Controller controller, TRequest request)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Checks the model the action bound by the validator named, with its asynchronous rules, and adds each value the binder refused under the name it was sent under.</summary>
    /// <remarks>
    /// The validator is the one the app registered. Return the result's <c>ToErrorResponse()</c> when it is not valid.
    /// MVC's own validation adds nothing to the result.
    /// </remarks>
    /// <param name="controller">The action's controller.</param>
    /// <param name="request">The model the action bound.</param>
    /// <param name="cancellation">Cancels the check.</param>
    /// <typeparam name="TRequest">The type of the model.</typeparam>
    /// <typeparam name="TValidator">The validator that checks it, the one the request names in the view.</typeparam>
    /// <returns>The check's result.</returns>
    public static Task<ValidationResult> ValidateAsync<TRequest, TValidator>(this Controller controller, TRequest request, CancellationToken cancellation = default)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

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
        => throw new NotImplementedException();

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
    public static Task<ValidationResult> ValidateAsync<TRequest, TValidator>(this Controller controller, TRequest request, string ruleSet, CancellationToken cancellation = default)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();
}
#endif
