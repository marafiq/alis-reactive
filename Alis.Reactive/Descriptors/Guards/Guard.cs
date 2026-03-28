using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// Abstract base for all guard descriptors in the JSON plan. Each concrete subclass
    /// serializes as a polymorphic object with a <c>kind</c> discriminator that the
    /// JavaScript runtime evaluates before executing a reaction or command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Guards form a composable tree: <see cref="ValueGuard"/> is a leaf condition,
    /// <see cref="AllGuard"/> and <see cref="AnyGuard"/> are logical combinators, and
    /// <see cref="InvertGuard"/> negates any subtree. <see cref="ConfirmGuard"/> is a
    /// special halting guard that prompts the user before proceeding.
    /// </para>
    /// <para>
    /// Serialized via <see cref="WriteOnlyPolymorphicConverter{T}"/> so that each subclass
    /// writes its own <c>kind</c> property without requiring <c>JsonDerivedType</c> attributes
    /// on the base class.
    /// </para>
    /// </remarks>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Guard>))]
    public abstract class Guard { }
}
