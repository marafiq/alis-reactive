using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// A JavaScript object the browser holds, declared member for member by its JavaScript names: a component, a
/// plugin, an object a member holds or a method returns, an item of a list, or what an event's listener receives.
/// </summary>
/// <remarks>
/// A declaration mirrors the object's typed API. Each property or method of the class declares one member, with
/// the door for its kind as its body, and the C# member's name in camel case is the JavaScript name unless
/// <c>name:</c> gives another. A member the API marks optional is declared <c>optional: true</c>. Reading, setting
/// or calling a required member the object lacks stops the pipeline that does it.
/// </remarks>
public abstract class JsObject
{
    /// <summary>Declares a property the browser only reads.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional: an absent one reads nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The property, a source of its value.</returns>
    protected ReadOnly<TValue> ReadOnly<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a property the browser reads and sets.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional: an absent one reads nothing, and a set writes it.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The property, a source of its value that a step also sets.</returns>
    protected Property<TValue> Property<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a property the browser only sets.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional: a set writes it whether the object has it or not.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The property, which a step sets.</returns>
    protected WriteOnly<TValue> WriteOnly<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a property that holds another object.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional.</param>
    /// <param name="nullable">True when the API types the object as null or undefined at times: its members read nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TObject">The declaration of the object the property holds.</typeparam>
    /// <returns>The object, whose members are read, set and called through its declaration.</returns>
    protected TObject LiveObject<TObject>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Declares a property that holds an array of objects.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional.</param>
    /// <param name="nullable">True when the API types the array as null or undefined at times: it reads nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TItem">The declaration of each object the array holds.</typeparam>
    /// <returns>The list, with the operations a list of objects takes.</returns>
    protected LiveObjects<TItem> LiveObjects<TItem>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns nothing, and calls it where the pipeline reaches the line.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    protected void Method(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns nothing, and calls it with its arguments where the pipeline reaches the line.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    protected void Method(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns a value, and calls it where the pipeline reaches the line.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing, and its result reads nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>What the call returned at that line, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> Method<TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns a value, and calls it with its arguments where the pipeline reaches the line.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing, and its result reads nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>What the call returned at that line, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> Method<TResult>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns a promise of nothing; the pipeline waits for it before its next step.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    protected void MethodAsync(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns a promise of nothing; the pipeline calls it with its arguments and waits for it before its next step.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    protected void MethodAsync(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns a promise of a value; the pipeline waits for the value before its next step.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing, and its result reads nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The value the promise gave, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> MethodAsync<TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns a promise of a value; the pipeline calls it with its arguments and waits for the value before its next step.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional: a call of an absent one runs nothing, and its result reads nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The value the promise gave, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> MethodAsync<TResult>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns another object, and calls it where the pipeline reaches the line.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional.</param>
    /// <param name="nullable">True when the API types the returned object as null or undefined at times: its members read nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
    /// <returns>The object the call returned, whose members are read, set and called through its declaration.</returns>
    protected TObject ReturnedObject<TObject>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns another object, and calls it with its arguments where the pipeline reaches the line.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional.</param>
    /// <param name="nullable">True when the API types the returned object as null or undefined at times: its members read nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TObject">The declaration of the object the method returns.</typeparam>
    /// <returns>The object the call returned, whose members are read, set and called through its declaration.</returns>
    protected TObject ReturnedObject<TObject>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Declares a method that takes no argument and returns an array of objects, and calls it where the pipeline reaches the line.</summary>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional.</param>
    /// <param name="nullable">True when the API types the returned array as null or undefined at times: it reads nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TItem">The declaration of each object the array holds.</typeparam>
    /// <returns>The list the call returned, with the operations a list of objects takes.</returns>
    protected LiveObjects<TItem> ReturnedObjects<TItem>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Declares a method that returns an array of objects, and calls it with its arguments where the pipeline reaches the line.</summary>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="name">The method's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the method optional.</param>
    /// <param name="nullable">True when the API types the returned array as null or undefined at times: it reads nothing while it holds nothing.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TItem">The declaration of each object the array holds.</typeparam>
    /// <returns>The list the call returned, with the operations a list of objects takes.</returns>
    protected LiveObjects<TItem> ReturnedObjects<TItem>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Calls the object itself, a function, with no argument, where the pipeline reaches the line.</summary>
    /// <remarks>An object that is no function stops the pipeline.</remarks>
    protected void CallItself() => throw new NotImplementedException();

    /// <summary>Calls the object itself, a function, with its arguments, where the pipeline reaches the line.</summary>
    /// <remarks>An object that is no function stops the pipeline.</remarks>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    protected void CallItself(IReadOnlyList<Arg> arguments) => throw new NotImplementedException();

    /// <summary>Calls the object itself, a function that returns a value, with its arguments, where the pipeline reaches the line.</summary>
    /// <remarks>An object that is no function stops the pipeline.</remarks>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <returns>What the call returned at that line, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> CallItself<TResult>(IReadOnlyList<Arg> arguments) => throw new NotImplementedException();

    /// <summary>Declares the argument a listener or a callback receives at a position.</summary>
    /// <remarks>
    /// Only the declaration of what an event's listener or a callback receives reads an argument by its position;
    /// on any other declaration the read is refused where the view renders.
    /// </remarks>
    /// <param name="position">The argument's position, the first at zero.</param>
    /// <returns>The argument, a source of its value.</returns>
    protected ReadOnly<TValue> Argument<TValue>(int position) => throw new NotImplementedException();

    /// <summary>Calls, with no argument, the function a listener or a callback receives at a position.</summary>
    /// <remarks>
    /// Only the declaration of what an event's listener or a callback receives calls an argument by its position;
    /// on any other declaration the call is refused where the view renders.
    /// </remarks>
    /// <param name="position">The function's position among the arguments, the first at zero.</param>
    protected void MethodAt(int position) => throw new NotImplementedException();

    /// <summary>Calls, with its arguments, the function a listener or a callback receives at a position.</summary>
    /// <remarks>
    /// Only the declaration of what an event's listener or a callback receives calls an argument by its position;
    /// on any other declaration the call is refused where the view renders.
    /// </remarks>
    /// <param name="position">The function's position among the arguments, the first at zero.</param>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    protected void MethodAt(int position, IReadOnlyList<Arg> arguments) => throw new NotImplementedException();

    /// <summary>Declares a property whose value is a function the object calls for an answer, such as a label's formatter.</summary>
    /// <param name="name">The property's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="optional">True when the API marks the property optional.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TArgs">The declaration of the first argument the object calls the function with.</typeparam>
    /// <typeparam name="TResult">The type of the answer the object expects.</typeparam>
    /// <returns>The callback, which a step sets to an answer.</returns>
    protected Callback<TArgs, TResult> Callback<TArgs, TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        where TArgs : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Makes an argument of a function the object calls for an answer.</summary>
    /// <remarks>
    /// The answer is an expression over the arguments the object calls the function with, read on each call. It
    /// holds no step and waits for nothing.
    /// </remarks>
    /// <param name="answer">The answer, read from the arguments of each call.</param>
    /// <typeparam name="TArgs">The declaration of the first argument the object calls the function with.</typeparam>
    /// <typeparam name="TResult">The type of the answer the object expects.</typeparam>
    /// <returns>The argument to list among a method's arguments.</returns>
    protected static Arg Answer<TArgs, TResult>(Func<TArgs, TypedSource<TResult>> answer)
        where TArgs : JsObject, new()
        => throw new NotImplementedException();

    /// <summary>Makes an argument of a value the declaration itself gives, the same on every call.</summary>
    /// <param name="value">The value the method receives.</param>
    /// <returns>The argument to list among a method's arguments.</returns>
    protected static Arg Fixed<TValue>(TValue value)
        where TValue : notnull
        => throw new NotImplementedException();

    /// <summary>Makes an argument of a source whose type derives from the type the method declares.</summary>
    /// <param name="value">The source the view passes.</param>
    /// <typeparam name="TDeclared">The type the object's API declares for the parameter.</typeparam>
    /// <typeparam name="TSource">The type of the view's source.</typeparam>
    /// <returns>The argument to list among a method's arguments.</returns>
    protected static Arg Pass<TDeclared, TSource>(TypedSource<TSource> value)
        where TSource : TDeclared
        => throw new NotImplementedException();

    /// <summary>Declares an event the object raises with no payload.</summary>
    /// <param name="name">The event's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event a view handles.</returns>
    protected Event Event(string? name = null, [CallerMemberName] string declaredBy = "") => throw new NotImplementedException();

    /// <summary>Declares an event the object raises with a payload.</summary>
    /// <param name="name">The event's JavaScript name, when it is not the C# member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
    /// <returns>The event a view handles, with its payload.</returns>
    protected Event<TArgs> Event<TArgs>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}
