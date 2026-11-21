#if NET48
using System.Text.Json.Serialization.Metadata;
using System.Web;

namespace Alis.Reactive.InputsAndIds;

internal static partial class InputShapes
{
    private static partial bool IsAHostsFile(JsonTypeInfo member) => member is JsonTypeInfo<HttpPostedFileBase>;
}
#endif
