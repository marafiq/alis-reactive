using System;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// A HEAD request: its routes, which read no body, its validation and what runs around it.
/// </summary>
public sealed class HeadRequest<TModel, TRequest>
{
    internal HeadRequest()
    {
    }

    /// <summary>Runs a pipeline when the server answers 2xx.</summary>
    /// <remarks>A HEAD answer has no body: the pipeline reads its status and its headers.</remarks>
    /// <param name="success">The route's pipeline, with the reply.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route, and <c>Chained</c> receives that reply.</returns>
    public HeadRequest<TModel, TRequest, Response<TRequest>> OnSuccess(Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status.</summary>
    /// <remarks>A HEAD answer has no body: the pipeline reads its status and its headers.</remarks>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> OnError(int status, Action<Response<TRequest>, Pipeline<TModel>> error) => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure and a redirect that ends on another origin. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Names the validator that checks what the request sends, before it is sent.</summary>
    /// <remarks>A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Names the validator and the rule set that check what the request sends, before it is sent.</summary>
    /// <remarks>The check runs the rules the validator declares inside that rule set and no other. A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <param name="ruleSet">The rule set's name, as the validator's <c>RuleSet</c> declares it.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> Validate<TValidator>(string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Sends the request without checking what it sends.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> NoValidation() => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the request starts, before it is sent.</summary>
    /// <remarks>A step of it that stops the pipeline sends nothing and runs no <c>Finally</c>.</remarks>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request's route, whatever the outcome.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Takes none of the request defaults the app configured at startup; the antiforgery token is still sent.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> NoDefaults() => throw new NotImplementedException();

    /// <summary>Shows the busy indicator on the elements named while the request runs, in place of the elements its success route writes.</summary>
    /// <param name="elementIds">The ids of the elements the indicator shows on; with none, it shows on the page.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotImplementedException();

    /// <summary>Shows no busy indicator for the request.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> NoBusy() => throw new NotImplementedException();

    /// <summary>Sends the request once, whatever retry the app configured at startup.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest> NoRetry() => throw new NotImplementedException();
}

/// <summary>
/// A HEAD request after its success route is written: what is left of its sentence, and the request it chains.
/// </summary>
public sealed class HeadRequest<TModel, TRequest, TReply>
{
    internal HeadRequest()
    {
    }

    /// <summary>Runs a pipeline when the server answers with one status.</summary>
    /// <remarks>A HEAD answer has no body: the pipeline reads its status and its headers.</remarks>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> OnError(int status, Action<Response<TRequest>, Pipeline<TModel>> error) => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure and a redirect that ends on another origin. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Names the validator that checks what the request sends, before it is sent.</summary>
    /// <remarks>A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Names the validator and the rule set that check what the request sends, before it is sent.</summary>
    /// <remarks>The check runs the rules the validator declares inside that rule set and no other. A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <param name="ruleSet">The rule set's name, as the validator's <c>RuleSet</c> declares it.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> Validate<TValidator>(string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Sends the request without checking what it sends.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> NoValidation() => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the request starts, before it is sent.</summary>
    /// <remarks>A step of it that stops the pipeline sends nothing and runs no <c>Finally</c>.</remarks>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request's route, whatever the outcome.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Takes none of the request defaults the app configured at startup; the antiforgery token is still sent.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotImplementedException();

    /// <summary>Shows the busy indicator on the elements named while the request runs, in place of the elements its success route writes.</summary>
    /// <param name="elementIds">The ids of the elements the indicator shows on; with none, it shows on the page.</param>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotImplementedException();

    /// <summary>Shows no busy indicator for the request.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> NoBusy() => throw new NotImplementedException();

    /// <summary>Sends the request once, whatever retry the app configured at startup.</summary>
    /// <returns>The request.</returns>
    public HeadRequest<TModel, TRequest, TReply> NoRetry() => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request succeeded and its <c>Finally</c> ran.</summary>
    /// <remarks>A request the pipeline sends reads the reply, and this door ends the request's sentence.</remarks>
    /// <param name="chained">The steps to run, with the success route's reply.</param>
    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotImplementedException();
}
