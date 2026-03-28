namespace Alis.Reactive
{
    /// <summary>
    /// Provides typed access to HTTP response body properties in success handlers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Created by <see cref="Builders.Requests.ResponseBuilder{TModel}.OnSuccess{TResponse}"/>
    /// and passed as the first parameter of the lambda. The instance is used only for
    /// compile-time type inference; its property values are never read.
    /// </para>
    /// <para>
    /// Use with <c>SetText</c> or <c>SetHtml</c> to bind response properties to DOM elements:
    /// <code>
    /// .OnSuccess&lt;ApiResponse&gt;((json, s) => s.Element("name").SetText(json, r => r.Data.Name))
    /// </code>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The response body type, providing typed expression paths.</typeparam>
    public sealed class ResponseBody<T> where T : class
    {
        internal T Instance { get; }

        internal ResponseBody(T instance)
        {
            Instance = instance;
        }
    }
}
