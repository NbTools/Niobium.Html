using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class JsonMatrix : MatrixBase<JsonMatrixCol>
{
    public const string LevelField = "$Lvl";

    public readonly Dictionary<string, JProperty?> ConstCols;

    private readonly string[]? IgnoreColumns;
    private readonly Func<object, string?>? ObjConverter;

    public int RowsCount { get; private set; }
    public bool Invariant => Cols.All(c => c.Count == RowsCount);

    public JsonMatrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null, Func<object, string?>? converter = null)
    {
        IgnoreColumns = ignoreColumns ?? [];
        ObjConverter = converter;

        ObjConverter = converter;
        ConstCols = [];
        RowsCount = 0;

        foreach (var col in predefinedColumns ?? Enumerable.Empty<string>())
            GetOrCreateCol(col, isHtml: false, out int _);

    }

    public void AddRow(IEnumerable<MatrixCell<JProperty>> keyValuePairs)
    {
        HashSet<int> remainingCols = new(Enumerable.Range(0, Cols.Count));

        foreach (MatrixCell<JProperty> cell in keyValuePairs)
        {
            if (IgnoreColumns?.Contains(cell.ColName) ?? false)
                continue;
            if (cell.Content is null)
                continue;

            JsonMatrixCol col = GetOrCreateCol(cell.ColName, cell.IsHtml, out var ind);
            if (cell.IsHtml && !col.IsHtml)
                col.IsHtml = true; //Html may not appear on first line where this column is created, can be set at any point, then is should not be reset back
            remainingCols.Remove(ind);
            col.Add(cell.Content);
        }

        foreach (int ind in remainingCols)
        {
            Cols[ind].Add(null);
        }

        RowsCount++;
    }

    public void AddJArray(JArray jarr) => AddJTokens(jarr);

    internal void AddJTokens(IEnumerable<JToken> tokens)
    {
        foreach (JToken jtoken in tokens)
        {
            if (jtoken is not JObject jobj)
                throw new Exception($"JArray contains {jtoken.GetType().Name}. Only JObjects are supported in {nameof(AddJArray)}");

            List<MatrixCell<JProperty>> cells = [];
            foreach (JToken item in jobj.Children())
            {
                if (item is not JProperty jprop)
                    throw new Exception($"Only JProperties are supported inside jobj.Children(). The child type was: {item.GetType().Name} in {nameof(AddJArray)}");

                //TODO: Handle complex values
                cells.Add(new MatrixCell<JProperty>(jprop.Name, jprop, IsHtml: false));

                //table.TT("tr", tr => tr.TV("th", jprop.Name).TT("td", td => Convert(jprop.Value, td, propertyHandler, jprop.Name))); //Recursive

            }
            AddRow(cells);
        }
    }

    public JProperty? this[string colName, int rowNum]
    {
        get
        {
            var col = GetColumn(colName);
            return col[rowNum];
        }
    }


    private JsonMatrixCol GetOrCreateCol(string key, bool isHtml, out int ind)
    {
        ind = Cols.FindIndex(c => c.Name == key);
        if (ind != -1)
            return Cols[ind];

        JsonMatrixCol newCol = new(key, RowsCount)
        {
            IsHtml = isHtml
        };
        Cols.Add(newCol);
        return newCol;
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
            IEnumerable<object> enObj => Objs2String(enObj),
            _ => (obj.ToString() ?? String.Empty, false),
        };
    }

    public static (string str, bool raw) Objs2String(IEnumerable<object> objects)
    {
        Matrix matr = new();
        var count = matr.AddManyObjects(objects);
        if (count > 0)
            return (matr.ToHtml(), true);
        else
            return (String.Empty, false);
    }

    public void ConstantColumnsToFile(string fileName) => File.WriteAllLines(fileName, ConstCols.Select(p => p.Key + ',' + p.Value));

    public Dictionary<string, JProperty?> RemoveConstantColumns()
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

    #region Html Functionality
    //https://developer.mozilla.org/en-US/docs/Web/HTML/Element/table

    public string ToHtml()
    {
        StringBuilder sb = new();
        Tag tag = Tag.Create(sb);
        ToHtml(tag);
        return sb.ToString();
    }


    public void ToHtml(Tag t, Func<JsonMatrix, int, bool> filter) => t.TT("table", t1 =>
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
                foreach ((string key, JProperty? val) in ConstCols.OrderBy(p => p.Key))
                {
                    t.TT("tr", t2 =>
                    {
                        t2.TV("td", key);
                        if (val != null)
                            JsonToHtml.Convert(val, t2, propertyHandler: null, propName: val?.Name); //Recursive
                        else
                            t2.TV("td", String.Empty);
                    });
                }
            });
        }
    }


    private void HtmlHeaders(Tag t)
    {
        foreach (JsonMatrixCol col in Cols)
            t.TV("th", col.Name);
    }

    private void HtmlRow(Tag t, int rowNum)
    {
        foreach (JsonMatrixCol col in Cols)
        {
            JToken? val = col.Cells[rowNum]?.Value;
            if (val != null)
                t.TT("td", t2 => JsonToHtml.Convert(val, t, propertyHandler: null, propName: null));  //TODO: support handler and names
            else
                t.TV("td", String.Empty);
        }
    }
    #endregion Html Functionality


    #region Csv Functionality
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

        JsonMatrixCol? levelCol = null;
        var lvlInd = Cols.FindIndex(c => c.Name == LevelField);
        if (lvlInd != -1)
        {
            levelCol = Cols[lvlInd];
            Cols.RemoveAt(lvlInd);
        }

        WriteRow(writer, Cols.Select(c => c.Name), null);
        for (int i = 0; i < RowsCount; ++i)
            WriteRow(writer, Cols.Select(c => c[i]?.Value.ToString() ?? String.Empty), levelCol?[i]?.Value.ToString());
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


