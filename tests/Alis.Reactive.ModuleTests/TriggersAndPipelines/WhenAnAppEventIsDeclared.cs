using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

[TestFixture]
public sealed class WhenAnAppEventIsDeclared
{
    [TestCase("")]
    [TestCase(" ")]
    public void a_blank_name_is_refused(string name)
        => Assert.That(() => AppEvent.Declare<ResidentFlagged>(name: name),
            Throws.ArgumentException.With.Message.EqualTo("An event needs a name. (Parameter 'name')"));

    [Test]
    public void a_declaration_outside_a_member_is_refused()
        => Assert.That(() => AppEvent.Declare(declaredBy: ""),
            Throws.ArgumentException.With.Message.EqualTo(
                "Declare an event as a member of a class, for example public static readonly AppEvent Saved = AppEvent.Declare(); (Parameter 'declaredBy')"));
}
