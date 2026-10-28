using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Members;

/// <summary>The mistakes a page can make in declaring or naming a component's members, each refused where the page renders.</summary>
public enum MemberMistake
{
    DeclareAnEventOnAReturnedObject,
    DeclareAnObjectForAMemberTheDeclarationLacks,
    DeclareAMethodForNoMember,
    SelectNoMemberOfTheRow,
    SelectACallOnAMember,
    SelectAPositionBeforeTheFirst,
}

/// <summary>A page that declares or names a member wrongly, the way its mistake asks.</summary>
public sealed class MemberRefusalsPage
{
    public MemberMistake Mistake { get; set; }
}

/// <summary>A row that rings, declared as the object a method returns.</summary>
public sealed class RingingRow : JsObject
{
    public Event Rung => Event();
}

/// <summary>The slate, declared with a method that returns a row holding an event.</summary>
public sealed class RingingRowSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public RingingRow RowAt(Arg<int> index) => Object<RingingRow>(index);
}

/// <summary>The slate, declared with its header named for a member the declaration does not have.</summary>
public sealed class MisnamedHeaderSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public SlateHeader Header => Object<SlateHeader>(declaredBy: "Heading");
}

/// <summary>The slate, declared with a method named for no member at all.</summary>
public sealed class UnnamedClearSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public void Clear() => Method(declaredBy: "");
}

/// <summary>A shift's beds, a list a field can select an item of.</summary>
public sealed class Shift
{
    public List<string> Beds { get; set; } = new();
}

/// <summary>A board with a header, as a page's own interface names it.</summary>
public interface IHeadedBoard
{
    SlateHeader Header { get; }
}

/// <summary>The slate, declared with its header through the interface it implements, explicitly.</summary>
public sealed class HeadedSlate : Component, IHeadedBoard
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    SlateHeader IHeadedBoard.Header => Object<SlateHeader>();
}

/// <summary>A slate whose header the page reaches through an interface.</summary>
public sealed class HeadedPage;
