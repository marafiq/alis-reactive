using System;
using System.Linq.Expressions;

namespace Alis.Reactive.InputsAndIds;

/// <summary>
/// The id the framework gives the input a member binds, for a view to name it: a label's <c>for</c>, or a read of the
/// input from another view.
/// </summary>
public static class IdGenerator
{
    /// <summary>Gets the id of the input a member binds when a view renders it with no field prefix.</summary>
    /// <remarks>
    /// The id is the class that declares the member, then MVC's id of the member's path, so every view that renders the
    /// member, whatever its model, gives its input this id: <c>m =&gt; m.Kin.Name</c>, whose <c>Name</c> the class
    /// <c>Alis.Care.NextOfKin</c> declares, has the id <c>Alis_Care_NextOfKin__Kin_Name</c>. A generic class is named
    /// with its type arguments, as C# writes it. A member an
    /// interface declares, or a lambda that names no member of the model, is refused.
    /// </remarks>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <typeparam name="TModel">The model the member path starts at.</typeparam>
    /// <typeparam name="TProp">The type of the member.</typeparam>
    /// <returns>The input's id.</returns>
    public static string For<TModel, TProp>(Expression<Func<TModel, TProp>> member)
    {
        var text = MvcText.Of(member);
        return MemberLambda.Of(member, text, text, typeof(TModel)).Scope + "__" + MvcText.IdFromName(text);
    }
}
