using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Members;

public enum MemberMistake
{
    DeclareAnEventOnAReturnedObject,
    DeclareAnObjectForAMemberTheDeclarationLacks,
    DeclareAMethodForNoMember,
    SelectNoMemberOfTheRow,
    SelectACallOnAMember,
    SelectAPositionBeforeTheFirst,
}

public sealed class MemberRefusalsPage
{
    public MemberMistake Mistake { get; set; }
}

public sealed class RingingRow : JsObject
{
    public Event Rung => Event();
}

public sealed class RingingRowSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public RingingRow RowAt(Arg<int> index) => Object<RingingRow>(index);
}

public sealed class MisnamedHeaderSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public SlateHeader Header => Object<SlateHeader>(declaredBy: "Heading");
}

public sealed class UnnamedClearSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public void Clear() => Method(declaredBy: "");
}

public sealed class Shift
{
    public List<string> Beds { get; set; } = new();
}

public interface IHeadedBoard
{
    SlateHeader Header { get; }
}

public sealed class HeadedSlate : Component, IHeadedBoard
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    SlateHeader IHeadedBoard.Header => Object<SlateHeader>();
}

public sealed class NamesPage;

public sealed class HeadedPage;
