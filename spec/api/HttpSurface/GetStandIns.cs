using System;
using Alis.Reactive.Components;
using Alis.Reactive.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Pipelines;

public static class GetStandIns
{
    extension<TRequest>(Response<TRequest> reply)
    {
        public ResponseHeaders Headers => throw new NotSupportedException();

        public ReadOnly<TRequest> Request => throw new NotSupportedException();
    }

    extension(Failure failed)
    {
        public ResponseHeaders Headers => throw new NotSupportedException();
    }

    public static GetRequest<TModel, TRequest, Response<TRequest, TBody>> OnSuccess<TModel, TRequest, TBody>(
        this GetRequest<TModel, TRequest> request, SuccessContent<TBody> content, Action<Response<TRequest, TBody>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(
        this GetRequest<TModel, TRequest> request, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> OnError<TModel, TRequest, TError>(
        this GetRequest<TModel, TRequest> request, int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> OnError<TModel, TRequest>(
        this GetRequest<TModel, TRequest> request, int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> Validate<TModel, TRequest, TValidator>(this GetRequest<TModel, TRequest> request)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> NoValidation<TModel, TRequest>(this GetRequest<TModel, TRequest> request) => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> WhileLoading<TModel, TRequest>(this GetRequest<TModel, TRequest> request, Action<Pipeline<TModel>> pipeline)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> Finally<TModel, TRequest>(this GetRequest<TModel, TRequest> request, Action<Pipeline<TModel>> pipeline)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> NoDefaults<TModel, TRequest>(this GetRequest<TModel, TRequest> request) => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> Busy<TModel, TRequest>(this GetRequest<TModel, TRequest> request, params string[] elementIds)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> NoBusy<TModel, TRequest>(this GetRequest<TModel, TRequest> request) => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> NoRetry<TModel, TRequest>(this GetRequest<TModel, TRequest> request) => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> OnError<TModel, TRequest, TReply, TError>(
        this GetRequest<TModel, TRequest, TReply> request, int status, ErrorContent<TError> content, Action<Response<TRequest, TError>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> OnError<TModel, TRequest, TReply>(
        this GetRequest<TModel, TRequest, TReply> request, int status, NoContent content, Action<Response<TRequest>, Pipeline<TModel>> error)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> Validate<TModel, TRequest, TReply, TValidator>(this GetRequest<TModel, TRequest, TReply> request)
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> NoValidation<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> WhileLoading<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request, Action<Pipeline<TModel>> pipeline)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> Finally<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request, Action<Pipeline<TModel>> pipeline)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> NoDefaults<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> Busy<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request, params string[] elementIds)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> NoBusy<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, TReply> NoRetry<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request)
        => throw new NotSupportedException();

    public static void Chained<TModel, TRequest, TReply>(this GetRequest<TModel, TRequest, TReply> request, Action<TReply, Pipeline<TModel>> chained)
        => throw new NotSupportedException();
}

public sealed class GetStages<TModel, TRequest>
{
    internal GetStages()
    {
    }

    public GetRequest<TModel, TRequest> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();
}

public sealed class GetStages<TModel, TRequest, TReply>
{
    internal GetStages()
    {
    }

    public GetRequest<TModel, TRequest, TReply> Validate<TValidator>()
        where TValidator : ReactiveValidator<TRequest>
        => throw new NotSupportedException();
}
