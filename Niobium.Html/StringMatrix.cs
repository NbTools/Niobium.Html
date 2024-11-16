using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Niobium.Html;

[DebuggerDisplay("[{Cols.Count}x{RowsCount}]")]
public class StringMatrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null, Func<object, string?>? converter = null,
    Stack<string>? parentPropNames = null, HtmlInterceptor<string?>? htmlInterceptor = null)
    : MatrixBase<string>(predefinedColumns, ignoreColumns, converter, parentPropNames, htmlInterceptor)
{
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

    public void TrySplitDateTimeColumn(string colName, string dateName, string timeName)
    {
        int ind = Cols.FindIndex(c => c.Name == colName);
        if (ind == -1) return;

        var dateCol = new MatrixCol<string>(dateName, 0);
        var timeCol = new MatrixCol<string>(timeName, 0);
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


    public void ToHtml(Tag t, Func<StringMatrix, int, bool> filter) => t.TT("table", t1 =>
    {
        t.TT("tr", HtmlHeaders);
        for (int i = 0; i < RowsCount; i++)
        {
            if (filter?.Invoke(this, i) == true)
                t.TT("tr", t2 => HtmlRow(t2, i));
        }
    }); //thead tbody

    public override void ToHtml(Tag t)
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
                foreach ((string key, string? val) in ConstCols.OrderBy(p => p.Key))
                {
                    t.TT("tr", t2 => t2.TV("td", key).TV("td", val));
                }
            });
        }
    }


    private void HtmlHeaders(Tag t)
    {
        foreach (MatrixCol<string> col in Cols)
            t.TV("th", col.Name);
    }

    private void HtmlRow(Tag t, int rowNum)
    {
        foreach (MatrixCol<string> col in Cols)
        {
            string? cellValue = col.Cells[rowNum];
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
    #endregion Csv Functionality
}

