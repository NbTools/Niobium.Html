using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Niobium.Html;

[DebuggerDisplay("[{Cols.Count}x{RowsCount}]")]
public class Matrix : MatrixBase<MatrixCol>
{
    public const string LevelField = "$Lvl";
    public readonly Dictionary<string, string> ConstCols;

    private readonly string[]? IgnoreColumns;
    private readonly Func<object, string?>? ObjConverter;
    private readonly HtmlInterceptor<string>? HtmlInterceptor;
    private readonly Stack<string> ParentPropNames;

    public int RowsCount { get; private set; }
    public bool Invariant => Cols.All(c => c.Count == RowsCount);

    public Matrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null,
        Func<object, string?>? converter = null, Stack<string>? parentPropNames = null, HtmlInterceptor<string>? htmlInterceptor = null) : base()
    {
        IgnoreColumns = ignoreColumns ?? [];
        ObjConverter = converter;
        HtmlInterceptor = htmlInterceptor;
        ParentPropNames = parentPropNames ?? new Stack<string>();
        ConstCols = [];
        RowsCount = 0;

        foreach (var col in predefinedColumns ?? Enumerable.Empty<string>())
            GetOrCreateCol(col, isHtml: false, out int _);
    }

    public void AddOneObject<T>(T obj, int level) where T : notnull => AddRow(GetProps(obj).Prepend(new MatrixCell<string>(LevelField, level.ToString(), false)));
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

    public void AddRow(IEnumerable<(string Key, string Value)> values) => AddRow(values.Select(p => new MatrixCell<string>(p.Key, p.Value, false)));
    public void AddRow(IDictionary<string, string?> keyValuePairs) => AddRow(keyValuePairs.Where(k => k.Value != null).Select(k => new MatrixCell<string>(k.Key, k.Value!, false)));
    public void AddRowIgnoreNulls(IEnumerable<(string Key, string? Value)> keyValuePairs) => AddRow(keyValuePairs.Where(p => p.Value != null).Select(p => new MatrixCell<string>(p.Key, p.Value!, false)));

    public void AddRow(IEnumerable<MatrixCell<string>> keyValuePairs)
    {
        HashSet<int> remainingCols = new(Enumerable.Range(0, Cols.Count));

        foreach (MatrixCell<string> cell in keyValuePairs)
        {
            if (IgnoreColumns?.Contains(cell.ColName) ?? false)
                continue;
            if (cell.Content is null)
                continue;

            MatrixCol col = GetOrCreateCol(cell.ColName, cell.IsHtml, out var ind);
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

    public string this[string colName, int rowNum]
    {
        get
        {
            var col = GetColumnFail(colName);
            return col[rowNum];
        }
    }

    private MatrixCol GetOrCreateCol(string key, bool isHtml, out int ind)
    {
        ind = Cols.FindIndex(c => c.Name == key);
        if (ind != -1)
            return Cols[ind];

        MatrixCol newCol = new(key, RowsCount)
        {
            IsHtml = isHtml
        };
        Cols.Add(newCol);
        return newCol;
    }

    private IEnumerable<MatrixCell<string>> GetProps(object obj)
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
            yield return new MatrixCell<string>(pi.Name, str, raw);
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
            Matrix matr = new();
            int _ = matr.AddManyObjects(objects);
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

        var dateCol = new MatrixCol(dateName, 0);
        var timeCol = new MatrixCol(timeName, 0);
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
        Tag tag = Tag.Create(sb);
        ToHtml(tag);
        return sb.ToString();
    }


    public void ToHtml(Tag t, Func<Matrix, int, bool> filter) => t.TT("table", t1 =>
    {
        t.TT("tr", HtmlHeaders);
        for (int i = 0; i < RowsCount; i++)
        {
            if (filter?.Invoke(this, i) == true)
                t.TT("tr", t2 => HtmlRow(t2, i));
        }
    }); //thead tbody

    public void ToHtml(Tag t)
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


    private void HtmlHeaders(Tag t)
    {
        foreach (MatrixCol col in Cols)
            t.TV("th", col.Name);
    }

    private void HtmlRow(Tag t, int rowNum)
    {
        foreach (MatrixCol col in Cols)
        {
            string cellValue = col.Cells[rowNum];
            t.TT("td", t1 =>
            {
                ParentPropNames.Push(col.Name);
                try
                {
                    if (!HtmlInterceptor?.Invoke(ParentPropNames, cellValue, t) ?? true)
                        t1.Text(cellValue, !col.IsHtml);
                }
                finally { ParentPropNames.Pop(); }
            });
        }
    }
    #endregion Html Functionality


    #region Csv Functionality
    public void LoadCsv(string csv)
    {
        var rdr = new StringReader(csv);
        LoadCsv(rdr);
    }

    public void LoadCsv(FileInfo file)
    {
        using StreamReader rdr = new(file.FullName);
        LoadCsv(rdr);
    }

    public void LoadCsv(TextReader stream)
    {
        StringBuilder bld = new();
        int counter = 0;
        string[] headers = [];
        string? line;
        while ((line = stream.ReadLine()) != null)
        {
            counter++;
            if (String.IsNullOrWhiteSpace(line))
                continue;

            if (headers.Length == 0) //Headers not read
            {
                headers = CsvTool.DeCsvLine(line, bld, ',', trim: true).ToArray();
                continue;
            }

            AddRow(headers.Zip(CsvTool.DeCsvLine(line, bld, ',', trim: true)));
        }
    }

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
        if (!Invariant) throw new InvalidOperationException("Matrix Invariant");

        MatrixCol? levelCol = null;
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

