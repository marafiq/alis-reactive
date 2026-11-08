using System;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;

namespace Alis.Reactive.Pipelines;

public static class RequestVerbs
{
    public static GetRequest<TModel, NoModel> Get<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest> Get<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<QueryParts<TRequest>> request)
        => throw new NotSupportedException();

    public static HeadRequest<TModel, NoModel> Head<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static HeadRequest<TModel, TRequest> Head<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<QueryParts<TRequest>> request)
        => throw new NotSupportedException();

    public static OptionsRequest<TModel, NoModel> Options<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static OptionsRequest<TModel, TRequest> Options<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<QueryParts<TRequest>> request)
        => throw new NotSupportedException();

    public static PostRequest<TModel, NoModel> Post<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static PostRequest<TModel, TRequest> Post<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<BodyParts<TRequest>> request)
        => throw new NotSupportedException();

    public static PutRequest<TModel, NoModel> Put<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static PutRequest<TModel, TRequest> Put<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<BodyParts<TRequest>> request)
        => throw new NotSupportedException();

    public static DeleteRequest<TModel, NoModel> Delete<TModel>(this Pipeline<TModel> pipeline, string url) => throw new NotSupportedException();

    public static DeleteRequest<TModel, TRequest> Delete<TModel, TRequest>(this Pipeline<TModel> pipeline, string url, Action<BodyParts<TRequest>> request)
        => throw new NotSupportedException();

    public static PageUrl<TPage> PageUrl<TModel, TPage>(this Pipeline<TModel> pipeline) => throw new NotSupportedException();

    public static PageUrl<TPage> PageUrl<TModel, TPage>(this Pipeline<TModel> pipeline, string template) => throw new NotSupportedException();

    public static void Parallel<TModel>(this Pipeline<TModel> pipeline, params Action<Pipeline<TModel>>[] branches) => throw new NotSupportedException();

    public static PageForm Form<TModel>(this Pipeline<TModel> pipeline, string formId) => throw new NotSupportedException();

    public static void Into<TModel>(this Pipeline<TModel> pipeline, string elementId, ITypedSource<Markup> html) => throw new NotSupportedException();
}
