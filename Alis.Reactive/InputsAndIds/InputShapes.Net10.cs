#if NET10_0_OR_GREATER
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;

namespace Alis.Reactive.InputsAndIds;

internal static partial class InputShapes
{
    private static partial bool IsAHostsFile(JsonTypeInfo member) => member is JsonTypeInfo<IFormFile>;
}
#endif
