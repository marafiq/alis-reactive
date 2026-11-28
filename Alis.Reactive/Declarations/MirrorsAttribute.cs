using System;

namespace Alis.Reactive.Declarations;

/// <summary>
/// Names the TypeScript type a declaration mirrors, so the build checks the declaration against it.
/// </summary>
/// <remarks>
/// The build reads the type from the app's TypeScript program and refuses a member the type lacks, a value type
/// that differs from the member's, and a presence or a nullability the type does not state.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MirrorsAttribute : Attribute
{
    /// <summary>Names the type the declaration mirrors.</summary>
    /// <param name="module">The module that declares the type: an npm package, a library of TypeScript or a file of the app.</param>
    /// <param name="name">The type's name in that module.</param>
    public MirrorsAttribute(string module, string name)
    {
        Module = module;
        Name = name;
    }

    /// <summary>Gets the module that declares the type.</summary>
    public string Module { get; }

    /// <summary>Gets the type's name in its module.</summary>
    public string Name { get; }
}
