namespace Alis.Reactive
{
    /// <summary>
    /// Phantom type for typed JSON response body access in success handlers.
    /// Same pattern as event payload phantoms — used only for compile-time
    /// type inference so ExpressionPathHelper generates "responseBody." prefixed paths.
    ///
    /// Created by OnSuccess&lt;T&gt; and passed through the lambda,
    /// just as CustomEvent&lt;T&gt; creates and passes a TPayload phantom.
    /// </summary>
    public sealed class ResponseBody<T> where T : class
    {
        internal T Instance { get; }

        internal ResponseBody(T instance)
        {
            Instance = instance;
        }
    }
}
