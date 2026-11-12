using System;
using Alis.Reactive.Elements;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// The steps of a pipeline around its requests: requests sent at once, a check with no request, a server's errors
/// shown, and a reply's HTML loaded.
/// </summary>
public static class RequestSteps
{
    /// <summary>Runs several pipelines at once, each sending its own request; the steps after this one run when every one has finished.</summary>
    /// <param name="pipeline">The pipeline the step is written in.</param>
    /// <param name="branches">The pipelines to run at once.</param>
    public static void Parallel<TModel>(this Pipeline<TModel> pipeline, params Action<Pipeline<TModel>>[] branches) => throw new NotImplementedException();

    /// <summary>Checks a container's inputs, as it sends them, by the validator registered for its model, with no request.</summary>
    /// <remarks>A check that fails shows its failures at the container and stops the pipeline.</remarks>
    /// <param name="pipeline">The pipeline the step is written in.</param>
    /// <param name="container">The container whose inputs are checked.</param>
    public static void Validate<TModel, TContainer>(this Pipeline<TModel> pipeline, RequestContainer<TContainer> container) => throw new NotImplementedException();

    /// <summary>Shows a server's validation errors as the request's failures: every message in its summary, and each at the input the request sent under its key.</summary>
    /// <param name="pipeline">The pipeline of a request's route.</param>
    /// <param name="problem">The source of the errors: the body of a 400 answer, or another route's.</param>
    public static void ShowValidationErrors<TModel>(this Pipeline<TModel> pipeline, ITypedSource<ValidationProblem> problem) => throw new NotImplementedException();

    /// <summary>Loads the HTML a reply read into an element, replacing what the element held.</summary>
    /// <remarks>What the element held is released first: its plans stop and its components leave.</remarks>
    /// <param name="pipeline">The pipeline of a request's route.</param>
    /// <param name="elementId">The id of the element the HTML is loaded into.</param>
    /// <param name="html">The source of the HTML: the body of a reply read as HTML.</param>
    public static void Into<TModel>(this Pipeline<TModel> pipeline, string elementId, ITypedSource<Markup> html) => throw new NotImplementedException();
}

/// <summary>
/// The door of a pipeline to an element of the page.
/// </summary>
public static class ElementSteps
{
    /// <summary>Reaches an element of the page by the id the view gave it.</summary>
    /// <param name="pipeline">The pipeline the step is written in.</param>
    /// <param name="id">The element's id.</param>
    /// <returns>The element.</returns>
    public static Element Element<TModel>(this Pipeline<TModel> pipeline, string id) => pipeline.Component<Element>(id);
}
