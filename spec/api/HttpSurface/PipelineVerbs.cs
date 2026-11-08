using System;
using Alis.Reactive.Requests;

namespace Alis.Reactive.Pipelines;

public sealed class PipelineVerbs<TModel>
{
    internal PipelineVerbs()
    {
    }

    public GetRequest<TModel, NoModel> Get(string url) => throw new NotSupportedException();

    public GetRequest<TModel, TRequest> Get<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotSupportedException();

    public HeadRequest<TModel, NoModel> Head(string url) => throw new NotSupportedException();

    public HeadRequest<TModel, TRequest> Head<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotSupportedException();

    public OptionsRequest<TModel, NoModel> Options(string url) => throw new NotSupportedException();

    public OptionsRequest<TModel, TRequest> Options<TRequest>(string url, Action<QueryParts<TRequest>> request) => throw new NotSupportedException();

    public PostRequest<TModel, NoModel> Post(string url) => throw new NotSupportedException();

    public PostRequest<TModel, TRequest> Post<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotSupportedException();

    public PutRequest<TModel, NoModel> Put(string url) => throw new NotSupportedException();

    public PutRequest<TModel, TRequest> Put<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotSupportedException();

    public DeleteRequest<TModel, NoModel> Delete(string url) => throw new NotSupportedException();

    public DeleteRequest<TModel, TRequest> Delete<TRequest>(string url, Action<BodyParts<TRequest>> request) => throw new NotSupportedException();

    public PageUrl<TPage> PageUrl<TPage>() => throw new NotSupportedException();

    public PageUrl<TPage> PageUrl<TPage>(string template) => throw new NotSupportedException();

    public RequestContainer<TContainer> Container<TContainer>(string containerId) => throw new NotSupportedException();
}
