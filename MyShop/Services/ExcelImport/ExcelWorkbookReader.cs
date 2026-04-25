using System.IO.Compression;
using System.Xml.Linq;

namespace MyShop.Services.ExcelImport;

internal sealed class ExcelWorkbookReader : IDisposable
{
    private readonly FileStream _fileStream;
    private readonly ZipArchive _archive;
    private readonly Dictionary<string, string> _sheetPathByName;
    private readonly List<string> _sharedStrings;

    private static readonly XNamespace MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

    public ExcelWorkbookReader(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _archive = new ZipArchive(_fileStream, ZipArchiveMode.Read);
        _sharedStrings = LoadSharedStrings();
        _sheetPathByName = LoadSheetPaths();
    }

    public List<ExcelRowData> ReadRows(string sheetName)
    {
        if (!_sheetPathByName.TryGetValue(sheetName, out var sheetPath))
        {
            return new List<ExcelRowData>();
        }

        var sheetEntry = _archive.GetEntry(sheetPath);
        if (sheetEntry == null)
        {
            return new List<ExcelRowData>();
        }

        var sheetDoc = LoadXml(sheetEntry);
        var rows = sheetDoc.Descendants(MainNs + "row").ToList();
        if (rows.Count == 0)
        {
            return new List<ExcelRowData>();
        }

        var headerRow = rows[0];
        var columnHeaders = BuildHeaderMap(headerRow);
        var result = new List<ExcelRowData>();

        foreach (var row in rows.Skip(1))
        {
            var values = BuildRowValues(row, columnHeaders);
            if (values.Count == 0 || values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            result.Add(new ExcelRowData(GetRowNumber(row), values));
        }

        return result;
    }

    public bool HasSheet(string sheetName)
    {
        return _sheetPathByName.ContainsKey(sheetName);
    }

    public List<string> GetHeaders(string sheetName)
    {
        if (!_sheetPathByName.TryGetValue(sheetName, out var sheetPath))
        {
            return new List<string>();
        }

        var sheetEntry = _archive.GetEntry(sheetPath);
        if (sheetEntry == null)
        {
            return new List<string>();
        }

        var sheetDoc = LoadXml(sheetEntry);
        var headerRow = sheetDoc.Descendants(MainNs + "row").FirstOrDefault();
        if (headerRow == null)
        {
            return new List<string>();
        }

        return BuildHeaderMap(headerRow).Values.ToList();
    }

    public void Dispose()
    {
        _archive.Dispose();
        _fileStream.Dispose();
    }

    private Dictionary<string, string> LoadSheetPaths()
    {
        var workbookEntry = _archive.GetEntry("xl/workbook.xml")
            ?? throw new InvalidOperationException("Invalid .xlsx file: xl/workbook.xml is missing.");
        var relsEntry = _archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new InvalidOperationException("Invalid .xlsx file: workbook relationships are missing.");

        var workbookDoc = LoadXml(workbookEntry);
        var relsDoc = LoadXml(relsEntry);

        var targetById = relsDoc
            .Descendants(PkgRelNs + "Relationship")
            .Where(r => r.Attribute("Id") != null && r.Attribute("Target") != null)
            .ToDictionary(
                r => r.Attribute("Id")!.Value,
                r => NormalizeWorksheetPath(r.Attribute("Target")!.Value),
                StringComparer.OrdinalIgnoreCase);

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sheetElements = workbookDoc
            .Descendants(MainNs + "sheet")
            .Where(s => s.Attribute("name") != null && s.Attribute(RelNs + "id") != null);

        foreach (var sheet in sheetElements)
        {
            var name = sheet.Attribute("name")!.Value;
            var relId = sheet.Attribute(RelNs + "id")!.Value;
            if (targetById.TryGetValue(relId, out var path))
            {
                map[name] = path;
            }
        }

        return map;
    }

    private List<string> LoadSharedStrings()
    {
        var sharedEntry = _archive.GetEntry("xl/sharedStrings.xml");
        if (sharedEntry == null)
        {
            return new List<string>();
        }

        var sharedDoc = LoadXml(sharedEntry);
        var stringItems = sharedDoc.Descendants(MainNs + "si");

        return stringItems
            .Select(item => string.Concat(item.Descendants(MainNs + "t").Select(t => t.Value)))
            .ToList();
    }

    private static string NormalizeWorksheetPath(string target)
    {
        if (target.StartsWith("/", StringComparison.Ordinal))
        {
            return target.TrimStart('/');
        }

        if (target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
        {
            return target;
        }

        return $"xl/{target}";
    }

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private Dictionary<string, string> BuildHeaderMap(XElement headerRow)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.Elements(MainNs + "c"))
        {
            var column = GetColumnName(cell.Attribute("r")?.Value);
            if (string.IsNullOrWhiteSpace(column))
            {
                continue;
            }

            var value = GetCellText(cell).Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                map[column] = value;
            }
        }

        return map;
    }

    private Dictionary<string, string> BuildRowValues(XElement row, Dictionary<string, string> columnHeaders)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cellByColumn = row.Elements(MainNs + "c")
            .Where(c => !string.IsNullOrWhiteSpace(c.Attribute("r")?.Value))
            .ToDictionary(c => GetColumnName(c.Attribute("r")!.Value), c => c, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in columnHeaders)
        {
            if (!cellByColumn.TryGetValue(entry.Key, out var cell))
            {
                values[entry.Value] = string.Empty;
                continue;
            }

            values[entry.Value] = GetCellText(cell).Trim();
        }

        return values;
    }

    private string GetCellText(XElement cell)
    {
        var dataType = cell.Attribute("t")?.Value;
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return cell.Element(MainNs + "v")?.Value ?? string.Empty;
        }

        return dataType switch
        {
            "s" => ReadSharedString(cell),
            "inlineStr" => ReadInlineString(cell),
            "b" => (cell.Element(MainNs + "v")?.Value ?? string.Empty) == "1" ? "TRUE" : "FALSE",
            _ => cell.Element(MainNs + "v")?.Value ?? string.Empty
        };
    }

    private string ReadSharedString(XElement cell)
    {
        var raw = cell.Element(MainNs + "v")?.Value;
        if (!int.TryParse(raw, out var index))
        {
            return string.Empty;
        }

        if (index < 0 || index >= _sharedStrings.Count)
        {
            return string.Empty;
        }

        return _sharedStrings[index];
    }

    private static string ReadInlineString(XElement cell)
    {
        var inline = cell.Element(MainNs + "is");
        if (inline == null)
        {
            return string.Empty;
        }

        return string.Concat(inline.Descendants(MainNs + "t").Select(t => t.Value));
    }

    private static int GetRowNumber(XElement row)
    {
        var raw = row.Attribute("r")?.Value;
        return int.TryParse(raw, out var number) ? number : 0;
    }

    private static string GetColumnName(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return string.Empty;
        }

        var chars = new string(cellReference
            .TakeWhile(char.IsLetter)
            .ToArray());

        return chars;
    }
}

internal sealed class ExcelRowData
{
    public ExcelRowData(int rowNumber, Dictionary<string, string> values)
    {
        RowNumber = rowNumber;
        Values = values;
    }

    public int RowNumber { get; }
    public Dictionary<string, string> Values { get; }

    public string GetValue(string headerName)
    {
        return Values.TryGetValue(headerName, out var value)
            ? value
            : string.Empty;
    }
}
