namespace Alis.Reactive
{
    /// <summary>
    /// A value read when a step runs, typed by C#: an object's property, what a method returns, an event's
    /// payload, a response, the URL or an array. It goes wherever the DSL takes a value, a method's
    /// argument included.
    /// </summary>
    /// <typeparam name="TProp">The value's type.</typeparam>
    public abstract class TypedSource<TProp> : Arg<TProp>
    {
    }
}
