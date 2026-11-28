using System;
using Alis.Reactive.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Pipelines;

public sealed class HeadRequest<TModel, TRequest>
{
    private HeadRequest()
    {
    }

    public HeadRequest<TModel, TRequest, Response<TRequest>> OnSuccess(Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> OnError(int status, Action<Response<TRequest>, Pipeline<TModel>> error) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> NoValidation() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> NoDefaults() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> NoBusy() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> NoRetry() => throw new NotSupportedException();
}

public sealed class HeadRequest<TModel, TRequest, TReply>
{
    private HeadRequest()
    {
    }

    public HeadRequest<TModel, TRequest, TReply> OnError(int status, Action<Response<TRequest>, Pipeline<TModel>> error) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> NoValidation() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> NoBusy() => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest, TReply> NoRetry() => throw new NotSupportedException();

    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotSupportedException();
}

public sealed class OptionsRequest<TModel, TRequest>
{
    private OptionsRequest()
    {
    }

    public OptionsRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, Response<TRequest>> OnSuccess(NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> NoValidation() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> NoDefaults() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> NoBusy() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> NoRetry() => throw new NotSupportedException();
}

public sealed class OptionsRequest<TModel, TRequest, TReply>
{
    private OptionsRequest()
    {
    }

    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> NoValidation() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> NoBusy() => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest, TReply> NoRetry() => throw new NotSupportedException();

    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotSupportedException();
}

public sealed class PostRequest<TModel, TRequest>
{
    private PostRequest()
    {
    }

    public PostRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, Response<TRequest>> OnSuccess(NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> NoValidation() => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> NoDefaults() => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> NoBusy() => throw new NotSupportedException();
}

public sealed class PostRequest<TModel, TRequest, TReply>
{
    private PostRequest()
    {
    }

    public PostRequest<TModel, TRequest, TReply> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> NoValidation() => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest, TReply> NoBusy() => throw new NotSupportedException();

    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotSupportedException();
}

public sealed class PutRequest<TModel, TRequest>
{
    private PutRequest()
    {
    }

    public PutRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, Response<TRequest>> OnSuccess(NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> NoValidation() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> NoDefaults() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> NoBusy() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> NoRetry() => throw new NotSupportedException();
}

public sealed class PutRequest<TModel, TRequest, TReply>
{
    private PutRequest()
    {
    }

    public PutRequest<TModel, TRequest, TReply> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> NoValidation() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> NoBusy() => throw new NotSupportedException();

    public PutRequest<TModel, TRequest, TReply> NoRetry() => throw new NotSupportedException();

    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotSupportedException();
}

public sealed class DeleteRequest<TModel, TRequest>
{
    private DeleteRequest()
    {
    }

    public DeleteRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, Response<TRequest>> OnSuccess(NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TBody>(Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> NoValidation() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> NoDefaults() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> Busy(params string[] elementIds) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> NoBusy() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> NoRetry() => throw new NotSupportedException();
}

public sealed class DeleteRequest<TModel, TRequest, TReply>
{
    private DeleteRequest()
    {
    }

    public DeleteRequest<TModel, TRequest, TReply> OnError<TError>(int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> OnError(int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> OnError<TError>(int status, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> OnError(Action<Failure, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> OnError<TError>(Action<Failure<TError>, Pipeline<TModel>> failed) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> NoValidation() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> WhileLoading(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> Finally(Action<Pipeline<TModel>> pipeline) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> NoDefaults() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> Busy(params string[] elementIds) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> NoBusy() => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest, TReply> NoRetry() => throw new NotSupportedException();

    public void Chained(Action<TReply, Pipeline<TModel>> chained) => throw new NotSupportedException();
}
