namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class GridModel
    {
        public decimal? MinAge { get; set; }
    }

    public class ResidentGridItem
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    /// <summary>
    /// Server-side grid response — SF Grid custom binding expects {result, count}.
    /// Set as dataSource directly: ej2.dataSource = {result: [...], count: N}.
    /// </summary>
    public class ResidentGridResponse
    {
        public List<ResidentGridItem> Result { get; set; } = new();
        public int Count { get; set; }
    }
}
