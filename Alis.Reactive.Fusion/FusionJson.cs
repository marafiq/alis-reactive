using Newtonsoft.Json.Serialization;

namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Newtonsoft naming for apps that render Fusion components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Syncfusion writes every component's options with Newtonsoft and the app's
    /// <c>JsonConvert.DefaultSettings</c>, so one global setting names both Syncfusion's
    /// options and the app's own data (data sources, template data). App data needs camel
    /// case to match the field names <c>Fields&lt;TItem&gt;(...)</c> emits. A few
    /// Syncfusion options pin a different name with <c>[JsonProperty]</c>: the Toast and
    /// Dialog position use <c>"X"</c> and <c>"Y"</c>.
    /// </para>
    /// <para>
    /// <c>CamelCasePropertyNamesContractResolver</c> renames pinned names too, so the
    /// position is dropped and the Toast falls back to the top left. This resolver camel
    /// cases every name that is not pinned and keeps every pinned name. Assign it once at
    /// startup; it is shared so Newtonsoft's contract cache is reused across calls:
    /// </para>
    /// <code>
    /// JsonConvert.DefaultSettings = () => new JsonSerializerSettings
    /// {
    ///     ContractResolver = FusionJson.ContractResolver
    /// };
    /// </code>
    /// </remarks>
    public static class FusionJson
    {
        /// <summary>
        /// Camel cases property names and dictionary keys, and keeps names declared with
        /// <c>[JsonProperty]</c>.
        /// </summary>
        public static IContractResolver ContractResolver { get; } = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = false
            }
        };
    }
}
