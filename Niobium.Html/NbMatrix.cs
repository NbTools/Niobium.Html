using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class NbMatrix
{
    public const string LevelField = "$Lvl";

    public readonly List<NbMatrixCol> Cols;
    public NbMatrixCol GetColumn(string colName) => Cols.SingleOrDefault(c => c.Name == colName) ?? throw new Exception($"Column '{colName}' was not found in Matrix");
    public readonly Dictionary<string, string> ConstCols;


    private readonly string[]? IgnoreColumns;
    private readonly Func<object, string?>? ObjConverter;


    public int RowsCount { get; private set; }
    public bool Invariant => Cols.All(c => c.Count == RowsCount);

    public NbMatrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null, Func<object, string?>? converter = null)
    {
        IgnoreColumns = ignoreColumns ?? Array.Empty<string>();
        ObjConverter = converter;

        ObjConverter = converter;
        Cols = new List<NbMatrixCol>();
        ConstCols = new Dictionary<string, string>();
        RowsCount = 0;

        foreach (var col in predefinedColumns ?? Enumerable.Empty<string>())
            GetOrCreateCol(col, isHtml: false, out int _);

    }

    public void AddOneObject<T>(T obj, int level) where T : notnull => AddRow(GetProps(obj).Prepend(new NbMatrixCell(LevelField, level.ToString(), false)));
    public void AddOneObject<T>(T obj) where T : notnull => AddRow(GetProps(obj));
    public int AddManyObjects<T>(IEnumerable<T> objects) where T : notnull
    {
        int count = 0;
        foreach (T obj in objects)
        {
            AddRow(GetProps(obj));
            count++;
        }
        return count;
    }

    public void AddRow(IEnumerable<(string Key, string Value)> values) => AddRow(values.Select(p => new NbMatrixCell(p.Key, p.Value, false)));
    public void AddRow(IDictionary<string, string?> keyValuePairs) => AddRow(keyValuePairs.Where(k => k.Value != null).Select(k => new NbMatrixCell(k.Key, k.Value!, false)));
    public void AddRowIgnoreNulls(IEnumerable<(string Key, string? Value)> keyValuePairs) => AddRow(keyValuePairs.Where(p => p.Value != null).Select(p => new NbMatrixCell(p.Key, p.Value!, false)));

    public void AddRow(IEnumerable<NbMatrixCell> keyValuePairs)
    {
        HashSet<int> remainingCols = new(Enumerable.Range(0, Cols.Count));

        foreach (NbMatrixCell cell in keyValuePairs)
        {
            if (IgnoreColumns?.Contains(cell.ColName) ?? false)
                continue;
            if (cell.Content is null)
                continue;

            NbMatrixCol col = GetOrCreateCol(cell.ColName, cell.IsHtml, out var ind);
            if (cell.IsHtml && !col.IsHtml)
                col.IsHtml = true; //Html may not appear on first line where this column is created, can be set at any point, then is should not be reset back
            remainingCols.Remove(ind);
            col.Add(cell.Content);
        }

        foreach (int ind in remainingCols)
        {
            Cols[ind].Add(String.Empty);
        }

        RowsCount++;
    }

    internal void AddJArray(JArray jarr)
    {
        foreach (JToken jobj in jarr)
        {
        }
    }


    public string this[string colName, int rowNum]
    {
        get
        {
            var col = GetColumn(colName);
            return col[rowNum];
        }
    }


    private NbMatrixCol GetOrCreateCol(string key, bool isHtml, out int ind)
    {
        ind = Cols.FindIndex(c => c.Name == key);
        if (ind != -1)
            return Cols[ind];

        NbMatrixCol newCol = new(key, RowsCount)
        {
            IsHtml = isHtml
        };
        Cols.Add(newCol);
        return newCol;
    }

    private IEnumerable<NbMatrixCell> GetProps(object obj)
    {
        var props = obj.GetType().GetProperties();
        foreach (PropertyInfo pi in props.Where(p => p.DeclaringType != p.ReflectedType)
            .Concat(props.Where(p => p.DeclaringType == p.ReflectedType))) //Base class first, Main class second
        {
            string str;
            bool raw = false;
            try
            {
                object? val = pi.GetValue(obj);
                (str, raw) = Obj2String(val);
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            yield return new NbMatrixCell(pi.Name, str, raw);
        }
    }

    public (string str, bool raw) Obj2String(object? obj)
    {
        if (obj == null)
            return (String.Empty, false);

        if (ObjConverter != null) //Don't try if there is not external converter
        {
            string? res = ObjConverter.Invoke(obj);
            if (res != null) //It couldn't convert - try ourselves
                return (res, false);
        }

        return obj switch
        {
            string str => (str, false),
            ICollection<object> collObj => Objs2String(collObj),
            IEnumerable<object> enObj => Objs2String(enObj.ToList()),
            _ => (obj.ToString() ?? String.Empty, false),
        };
    }

    public (string str, bool raw) Objs2String(ICollection<object> objects)
    {
        if (objects.Count == 0)
            return (String.Empty, false);
        else if (objects.Count == 1)
            return Obj2String(objects.First());
        else
        {
            NbMatrix matr = new();
            int count = matr.AddManyObjects(objects);
            return (matr.ToHtml(), true);
        }
    }

    public void ConstantColumnsToFile(string fileName) => File.WriteAllLines(fileName, ConstCols.Select(p => p.Key + ',' + p.Value));

    public Dictionary<string, string> RemoveConstantColumns()
    {
        if (RowsCount == 0) return ConstCols;

        Stack<int> colInds = new();
        for (int i = 0; i < Cols.Count; ++i)
        {
            if (Cols[i].IsConst())
                colInds.Push(i);
        }

        foreach (int ind in colInds)
        {
            ConstCols.Add(Cols[ind].Name, Cols[ind][0]);
            Cols.RemoveAt(ind);
        }
        return ConstCols;
    }

    public void TrySplitDateTimeColumn(string colName, string dateName, string timeName)
    {
        int ind = Cols.FindIndex(c => c.Name == colName);
        if (ind == -1) return;

        var dateCol = new NbMatrixCol(dateName, 0);
        var timeCol = new NbMatrixCol(timeName, 0);
        var cult = new CultureInfo("en-UK");

        foreach (var line in Cols[ind].Cells)
        {
            if (DateTime.TryParse(line, out DateTime dateTime))
            {
                dateCol.Add(dateTime.ToString("dd/MM/yyyy", cult));
                timeCol.Add(dateTime.ToString("HH:mm:ss", cult));
            }
            else
            {
                dateCol.Add(line);
                timeCol.Add(String.Empty);
            }
        }
        Cols.RemoveAt(ind);
        Cols.Insert(ind, timeCol);
        Cols.Insert(ind, dateCol);
    }

    #region Html Functionality
    //https://developer.mozilla.org/en-US/docs/Web/HTML/Element/table

    public string ToHtml()
    {
        StringBuilder sb = new();
        NbTag tag = NbTag.Create(sb);
        ToHtml(tag);
        return sb.ToString();
    }


    public void ToHtml(NbTag t, Func<NbMatrix, int, bool> filter) => t.TT("table", t1 =>
    {
        t.TT("tr", HtmlHeaders);
        for (int i = 0; i < RowsCount; i++)
        {
            if (filter?.Invoke(this, i) == true)
                t.TT("tr", t2 => HtmlRow(t2, i));
        }
    }); //thead tbody


    public void ToHtml(NbTag t)
    {
        t.TT("table", t1 =>
        {
            t.TT("tr", HtmlHeaders);
            for (int i = 0; i < RowsCount; i++)
                t.TT("tr", t2 => HtmlRow(t2, i));
        });

        if (ConstCols.Count > 0)
        {
            t.p(" ");
            t.h2("Constant Headers");
            t.TT("table", t1 =>
            {
                t.TT("tr", t2 => t2.TV("th", "Name").TV("th", "Value"));
                foreach ((string key, string val) in ConstCols.OrderBy(p => p.Key))
                {
                    t.TT("tr", t2 => t2.TV("td", key).TV("td", val));
                }
            });
        }
    }


    private void HtmlHeaders(NbTag t)
    {
        foreach (NbMatrixCol col in Cols)
            t.TV("th", col.Name);
    }

    private void HtmlRow(NbTag t, int rowNum)
    {
        foreach (NbMatrixCol col in Cols)
            t.TV("td", col.Cells[rowNum], encode: !col.IsHtml);
    }
    #endregion Html Functionality


    #region Csv Functionality
    /*public void ReadCsv(string fileName)
    {
        StringBuilder bld = new();
        using StreamReader rdr = new(fileName);

        int counter = 0;
        string[] headers = Array.Empty<string>();
        while (!rdr.EndOfStream)
        {
            counter++;

            string? line = rdr.ReadLine();
            if (String.IsNullOrWhiteSpace(line))
                continue;

            if (headers.Length == 0) //Headers not read
            {
                headers = NbExt.DeCsvLine(line, bld, ',', trim: true).ToArray();
                continue;
            }

            AddRow(headers.Zip(NbExt.DeCsvLine(line, bld, ',', trim: true)));
        }
    }*/

    public void ToCsv(string fileName)
    {
        using StreamWriter writer = new(fileName);
        ToCsv(writer);
    }

    public string ToCsv()
    {
        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        ToCsv(writer);
        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public void ToCsv(TextWriter writer)
    {
        if (!Invariant) throw new Exception("FtMatrix Invariant");

        NbMatrixCol? levelCol = null;
        var lvlInd = Cols.FindIndex(c => c.Name == LevelField);
        if (lvlInd != -1)
        {
            levelCol = Cols[lvlInd];
            Cols.RemoveAt(lvlInd);
        }

        WriteRow(writer, Cols.Select(c => c.Name), null);
        for (int i = 0; i < RowsCount; ++i)
            WriteRow(writer, Cols.Select(c => c[i]), levelCol?[i]);
    }

    private static void WriteRow(TextWriter writer, IEnumerable<string> cols, string? level)
    {
        bool first = true;
        foreach (string txt in cols)
        {
            if (first)
            {
                first = false;
                if (level != null && Int32.TryParse(level, out int margin))
                    writer.Write(new String(' ', margin * 3));  //Two spaces per level
            }
            else
                writer.Write(',');

            writer.Write(ToCsvString(txt));
        }
        writer.WriteLine();
    }

    internal static string ToCsvString(string str)
    {
        if (String.IsNullOrEmpty(str))
            return str;

        str = str.Replace("\"", "\"\"").Replace("\r\n", " "); //Double quotes
        if (str.Contains(',') || str.Contains('"'))
            return "\"" + str + "\"";
        else
            return str;
    }
    #endregion Csv Functionality
}

public record NbMatrixCell(string ColName, string Content, bool IsHtml);

public class NbMatrixCol
{
    public string Name { get; private set; }
    private readonly List<string> _Cells;
    public bool IsHtml = false; //Encode by default

    public NbMatrixCol(string name, int emptyCells)
    {
        Name = name;
        _Cells = new List<string>();
        _Cells.AddRange(Enumerable.Repeat(String.Empty, emptyCells));
    }

    public override string ToString() => $"{Name} {_Cells.Count}";

    public ReadOnlyCollection<string> Cells => _Cells.AsReadOnly();
    public string this[int ind] => _Cells[ind];
    public void Add(string val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    public bool IsConst() //Are column's values all the same?
    {
        bool first = true;
        string? prev = null;
        foreach (string val in _Cells)
        {
            if (first)
            {
                prev = val;
                first = false;
            }
            else
            {
                if (prev != val)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Truncate all the cells in the column so the text fits in symbolCount symbols
    /// </summary>
    /// <param name="symbolCount"></param>
    public void Truncate(int symbolCount)
    {
        for (int i = 0; i < _Cells.Count; i++)
        {
            if (_Cells[i].Length > symbolCount - 3)
                _Cells[i] = string.Concat(_Cells[i].AsSpan(0, symbolCount - 3), "...");
        }
    }

    public HashSet<string> ReplaceByDictInList(Dictionary<string, string> dict, char separator)
    {
        HashSet<string> nonResolved = new();

        for (int i = 0; i < _Cells.Count; i++)
        {
            string val = _Cells[i];
            foreach (string listItem in val.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (dict.TryGetValue(listItem, out string? replacement))
                    val = val.Replace(listItem, replacement);
                else
                    nonResolved.Add(listItem);
            }
            _Cells[i] = val;
        }
        return nonResolved;
    }

    public HashSet<string> ReplaceByDictInList(Dictionary<int, string> dict, char separator)
    {
        HashSet<string> nonResolved = new();

        for (int i = 0; i < _Cells.Count; i++)
        {
            string val = _Cells[i];
            foreach (string listItem in val.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (dict.TryGetValue(Int32.Parse(listItem), out string? replacement))
                    val = val.Replace(listItem, replacement);
                else
                    nonResolved.Add(listItem);
            }
            _Cells[i] = val;
        }
        return nonResolved;
    }
}
