using System;

namespace Alis.Reactive.Declarations;

/// <summary>
/// Exempts one member from the build's typed-surface check (ALIS009), with the reason it is untyped.
/// </summary>
/// <remarks>
/// ALIS009 fails the build on a public member that takes or returns <c>object</c> or <see cref="Type"/>, or that
/// takes a string named for a member, an event, a method or a path. Put this on a member whose untyped shape is
/// deliberate, such as the JavaScript names a vendor is described by. An empty reason exempts nothing and fails
/// the build.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
public sealed class TypedDslExemptionAttribute : Attribute
{
    /// <summary>Exempts the member for the reason given.</summary>
    /// <param name="reason">Why the member is untyped.</param>
    public TypedDslExemptionAttribute(string reason) => Reason = reason;

    /// <summary>Gets why the member is untyped.</summary>
    public string Reason { get; }
}
