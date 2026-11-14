using System;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// An OPTIONS request: its routes, its validation and what runs around it.
/// </summary>
public sealed class OptionsRequest<TModel, TRequest>
{
    internal OptionsRequest()
    {
    }

    /// <summary>Runs a pipeline when the server answers 2xx with a body of the kind named.</summary>
    /// <remarks>A 2xx answer whose body is not of that kind runs the catch-all route.</remarks>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="success">The route's pipeline, with the reply.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route, and <c>Chained</c> receives that reply.</returns>
    public OptionsRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx, without reading the body.</summary>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="success">The route's pipeline, with the reply.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route, and <c>Chained</c> receives that reply.</returns>
    public OptionsRequest<TModel, TRequest, Response<TRequest>> OnSuccess(NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers 2xx with a JSON body, read as <typeparamref name="TBody"/>.</summary>
    /// <remarks>A 2xx answer whose body is not JSON runs the catch-all route.</remarks>
    /// <param name="success">The route's pipeline, with the reply.</param>
    /// <returns>The request, carrying its reply's type: it takes no second success route, and <c>Chained</c> receives that reply.</returns>
    public OptionsRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status, reading the body as the kind named.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status, without reading the body.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status and a JSON body, read as <typeparamref name="TError"/>.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure, a redirect that ends on another origin, and a 2xx answer whose body is not of the success route's kind. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes, and reads the JSON body of an answer whose status is not 2xx as <typeparamref name="TError"/>.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure, a redirect that ends on another origin, and a 2xx answer whose body is not of the success route's kind. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Names the validator that checks what the request sends, before it is sent.</summary>
    /// <remarks>A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Names the validator and the rule set that check what the request sends, before it is sent.</summary>
    /// <remarks>The check runs the rules the validator declares inside that rule set and no other. A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <param name="ruleSet">The rule set's name, as the validator's <c>RuleSet</c> declares it.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> Validate<TValidator>(string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Sends the request without checking what it sends.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> NoValidation() => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the request starts, before it is sent.</summary>
    /// <remarks>A step of it that stops the pipeline sends nothing and runs no <c>Finally</c>.</remarks>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request's route, whatever the outcome.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Takes none of the request defaults the app configured at startup; the antiforgery token is still sent.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> NoDefaults() => throw new NotImplementedException();

    /// <summary>Shows the busy indicator on the elements named while the request runs, in place of the elements its success route writes.</summary>
    /// <param name="elementIds">The ids of the elements the indicator shows on; with none, it shows on the page.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotImplementedException();

    /// <summary>Shows no busy indicator for the request.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> NoBusy() => throw new NotImplementedException();

    /// <summary>Sends the request once, whatever retry the app configured at startup.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest> NoRetry() => throw new NotImplementedException();
}

/// <summary>
/// An OPTIONS request after its success route is written: what is left of its sentence, and the request it chains.
/// </summary>
public sealed class OptionsRequest<TModel, TRequest, TReply>
{
    internal OptionsRequest()
    {
    }

    /// <summary>Runs a pipeline when the server answers with one status, reading the body as the kind named.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status, without reading the body.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers with one status and a JSON body, read as <typeparamref name="TError"/>.</summary>
    /// <param name="status">The status the route is for, from 300 to 599; a request names one route a status.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure, a redirect that ends on another origin, and a 2xx answer whose body is not of the success route's kind. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome no other route takes, and reads the JSON body of an answer whose status is not 2xx as <typeparamref name="TError"/>.</summary>
    /// <remarks>The outcomes are a status with no route of its own, a network failure, a redirect that ends on another origin, and a 2xx answer whose body is not of the success route's kind. A request takes one catch-all.</remarks>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotImplementedException();

    /// <summary>Names the validator that checks what the request sends, before it is sent.</summary>
    /// <remarks>A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Names the validator and the rule set that check what the request sends, before it is sent.</summary>
    /// <remarks>The check runs the rules the validator declares inside that rule set and no other. A check that fails shows its failures at the request's container, sends nothing and runs neither <c>WhileLoading</c> nor <c>Finally</c>.</remarks>
    /// <param name="ruleSet">The rule set's name, as the validator's <c>RuleSet</c> declares it.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> Validate<TValidator>(string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotImplementedException();

    /// <summary>Sends the request without checking what it sends.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> NoValidation() => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the request starts, before it is sent.</summary>
    /// <remarks>A step of it that stops the pipeline sends nothing and runs no <c>Finally</c>.</remarks>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request's route, whatever the outcome.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Takes none of the request defaults the app configured at startup; the antiforgery token is still sent.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotImplementedException();

    /// <summary>Shows the busy indicator on the elements named while the request runs, in place of the elements its success route writes.</summary>
    /// <param name="elementIds">The ids of the elements the indicator shows on; with none, it shows on the page.</param>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotImplementedException();

    /// <summary>Shows no busy indicator for the request.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> NoBusy() => throw new NotImplementedException();

    /// <summary>Sends the request once, whatever retry the app configured at startup.</summary>
    /// <returns>The request.</returns>
    public OptionsRequest<TModel, TRequest, TReply> NoRetry() => throw new NotImplementedException();

    /// <summary>Runs a pipeline after the request succeeded and its <c>Finally</c> ran.</summary>
    /// <remarks>A request the pipeline sends reads the reply, and this door ends the request's sentence.</remarks>
    /// <param name="chained">The steps to run, with the success route's reply.</param>
    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotImplementedException();
}
