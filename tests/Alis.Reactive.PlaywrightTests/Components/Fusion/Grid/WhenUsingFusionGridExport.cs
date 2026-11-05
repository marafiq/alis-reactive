using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridExport : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterExport";
    private const string CensusId = "monthly-census";

    private const int CensusDaysColumn = 5;

    private static readonly string[] OctoberCsv =
    {
        "Resident,Suite,Care level,Census days",
        "Dorothy Fleming,201,Assisted Living,31",
        "Harold Chen,204,Memory Care,31",
        "Lucia Romero,207,Independent,31",
        "Margaret Hale,210,Assisted Living,31",
        "Theodore Banks,213,Skilled Nursing,20",
        "Vera Lindqvist,216,Memory Care,31"
    };

    private static readonly string[] OctoberResidents =
        { "Dorothy Fleming", "Harold Chen", "Lucia Romero", "Margaret Hale", "Theodore Banks", "Vera Lindqvist" };

    private PagePlan<RosterExportModel> _plan = null!;

    private ILocator Rows => Page.Locator($"#{CensusId} .e-row");
    private ILocator CensusDays(string resident)
        => Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{CensusDaysColumn}']");
    private ILocator Summary => Page.Locator("#census-summary");
    private ILocator DownloadExcel => Page.Locator("#download-census-excel");
    private ILocator DownloadCsv => Page.Locator("#download-census-csv");
    private ILocator DownloadPdf => Page.Locator("#download-census-pdf");
    private NativeDropDownLocator Month => _plan.NativeDropDown(m => m.Month);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RosterExportModel>.FromPage(Page);
        await Expect(Summary).ToHaveTextAsync("October 2026: 6 residents, 175 census days.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    private async Task<(string Name, byte[] Content)> Download(ILocator button)
    {
        var download = await Page.RunAndWaitForDownloadAsync(async () => await ClickWhenStable(button));
        var path = await download.PathAsync();
        return (download.SuggestedFilename, await File.ReadAllBytesAsync(path));
    }

    private static string[] CsvLines(byte[] csv)
        => Encoding.UTF8.GetString(csv).TrimStart('﻿').Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

    private static string WorkbookStrings(byte[] xlsx)
    {
        using var workbook = new ZipArchive(new MemoryStream(xlsx));
        using var reader = new StreamReader(workbook.GetEntry("xl/sharedStrings.xml")!.Open());
        return reader.ReadToEnd();
    }

    private static List<string> PdfTexts(byte[] pdf)
    {
        var raw = Encoding.Latin1.GetString(pdf);
        var texts = new List<string>();
        foreach (Match stream in Regex.Matches(raw, @"/Filter /FlateDecode.*?stream\r?\n(.*?)\r?\nendstream", RegexOptions.Singleline))
        {
            var body = stream.Groups[1];
            using var inflate = new ZLibStream(new MemoryStream(pdf, body.Index, body.Length), CompressionMode.Decompress);
            using var reader = new StreamReader(inflate, Encoding.Latin1);
            texts.AddRange(Regex.Matches(reader.ReadToEnd(), @"\(([^)]*)\)").Select(text => text.Groups[1].Value));
        }
        return texts;
    }

    [Test]
    public async Task the_census_opens_on_octobers_residents_and_days()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Dorothy Fleming");
        await Expect(Rows.Nth(4)).ToContainTextAsync("Theodore Banks");
        await Expect(CensusDays("Theodore Banks")).ToHaveTextAsync("20");
        await Expect(CensusDays("Vera Lindqvist")).ToHaveTextAsync("31");
        await Expect(Month.Input).ToHaveValueAsync("2026-10");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_csv_download_holds_the_census_row_for_row()
    {
        await NavigateAndBoot();

        var (name, content) = await Download(DownloadCsv);

        Assert.That(name, Is.EqualTo("Export.csv"));
        Assert.That(CsvLines(content), Is.EqualTo(OctoberCsv));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_excel_download_is_a_workbook_of_the_census()
    {
        await NavigateAndBoot();

        var (name, content) = await Download(DownloadExcel);

        Assert.That(name, Is.EqualTo("Export.xlsx"));
        var strings = WorkbookStrings(content);
        Assert.That(strings, Does.Contain("<t>Census days</t>"));
        foreach (var resident in OctoberResidents)
            Assert.That(strings, Does.Contain($"<t>{resident}</t>"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pdf_download_is_a_document_of_the_census()
    {
        await NavigateAndBoot();

        var (name, content) = await Download(DownloadPdf);

        Assert.That(name, Is.EqualTo("Export.pdf"));
        Assert.That(Encoding.Latin1.GetString(content, 0, 5), Is.EqualTo("%PDF-"));
        var texts = PdfTexts(content);
        Assert.That(texts, Does.Contain("Census days"));
        Assert.That(texts, Is.SupersetOf(OctoberResidents));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_september_downloads_septembers_census()
    {
        await NavigateAndBoot();
        await Month.Select("September 2026");
        await Expect(Summary).ToHaveTextAsync("September 2026: 6 residents, 164 census days.", new() { Timeout = 10000 });
        await Expect(Rows.Nth(4)).ToContainTextAsync("Samuel Ortiz", new() { Timeout = 10000 });

        var (_, content) = await Download(DownloadCsv);

        var lines = CsvLines(content);
        Assert.That(lines, Does.Contain("Samuel Ortiz,213,Skilled Nursing,14"));
        Assert.That(lines, Does.Contain("Dorothy Fleming,201,Assisted Living,30"));
        Assert.That(lines.Any(line => line.StartsWith("Theodore Banks")), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_month_still_open_is_refused_and_october_stays_on_the_page()
    {
        await NavigateAndBoot();

        await Month.Select("November 2026");

        await Expect(Summary).ToHaveTextAsync("November's census is still open; it closes on November 30.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(6);
        await Expect(Rows.Nth(4)).ToContainTextAsync("Theodore Banks");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }
}
