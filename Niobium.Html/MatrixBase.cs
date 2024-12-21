using System.Diagnostics.CodeAnalysis;

namespace Niobium.Html;

public abstract class MatrixBase<T> where T : class
{
    public readonly List<MatrixCol<T>> Cols = [];
    public const string LevelField = "$Lvl";
    public readonly Dictionary<string, T?> ConstCols;

    protected readonly string[]? IgnoreColumns;
    protected readonly Func<object, string?>? ObjConverter;
    protected readonly HtmlInterceptor<string?>? HtmlInterceptor;
    protected readonly Stack<string> ParentPropNames;

    public int RowsCount { get; protected set; }
    public bool Invariant => Cols.All(c => c.Count == RowsCount);

    public MatrixBase(string[]? predefinedColumns = null, string[]? ignoreColumns = null,
        Func<object, string?>? converter = null, Stack<string>? parentPropNames = null, HtmlInterceptor<string?>? htmlInterceptor = null) : base()
    {
        IgnoreColumns = ignoreColumns ?? [];
        ObjConverter = converter;
        HtmlInterceptor = htmlInterceptor;
        ParentPropNames = parentPropNames ?? new Stack<string>();

        ObjConverter = converter;
        ConstCols = [];
        RowsCount = 0;

        foreach (var col in predefinedColumns ?? Enumerable.Empty<string>())
            GetOrCreateCol(col, isHtml: false, out int _);
    }

    protected MatrixCol<T> GetOrCreateCol(string key, bool isHtml, out int ind)
    {
        ind = Cols.FindIndex(c => c.Name == key);
        if (ind != -1)
            return Cols[ind];

        MatrixCol<T> newCol = new(key, RowsCount)
        {
            IsHtml = isHtml
        };
        Cols.Add(newCol);
        return newCol;
    }

    public void AddRow(IEnumerable<MatrixCell<T>> keyValuePairs)
    {
        HashSet<int> remainingCols = new(Enumerable.Range(0, Cols.Count));

        foreach (MatrixCell<T> cell in keyValuePairs)
        {
            if (IgnoreColumns?.Contains(cell.ColName) ?? false)
                continue;
            if (cell.Content is null)
                continue;

            MatrixCol<T> col = GetOrCreateCol(cell.ColName, cell.IsHtml, out var ind);
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

    public bool TryGetColumn(string colName, [NotNullWhen(true)] out MatrixCol<T>? col)
    {
        col = Cols.SingleOrDefault(c => c.Name == colName);
        return col != null;
    }

    public string? this[string colName, int rowNum]
    {
        get
        {
            var col = GetColumnFail(colName);
            return col[rowNum];
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
            IEnumerable<object> enObj => Objs2String(enObj),
            IDictionary<string, string> strDict => Str2StrDict(strDict),  //TODO: support dicts of objects
            object theObj => TheObj2String(theObj),
            _ => (obj.ToString() ?? String.Empty, false),
        };
    }

    public static (string str, bool raw) Objs2String(IEnumerable<object> objects)
    {
        StringMatrix matr = new();
        var count = matr.AddManyObjects(objects);
        if (count > 0)
            return (matr.ToHtml(), true);
        else
            return (String.Empty, false);
    }

    public (string str, bool raw) Str2StrDict(IDictionary<string, string> objects)
    {
        if (objects.Count == 0)
            return (String.Empty, false);

        JsonObject jObj = new(HtmlInterceptor, ParentPropNames);
        using StringWriter stream = new();
        XTag bodyTag = new(stream); //, null, 1
        jObj.Convert(objects, bodyTag);
        return (stream.ToString(), true);
    }

    public (string str, bool raw) TheObj2String(object obj)
    {
        JsonObject jObj = new(HtmlInterceptor, ParentPropNames);
        using StringWriter stream = new();
        XTag bodyTag = new(stream); //, null, 1
        jObj.Convert(obj, bodyTag);
        return (stream.ToString(), true);
    }

    public void ConstantColumnsToFile(string fileName) => File.WriteAllLines(fileName, ConstCols.Select(p => p.Key + ',' + p.Value));

    public Dictionary<string, T?> RemoveConstantColumns()
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
            ConstCols.Add(Cols[ind].Name, Cols[ind].GetField(0));
            Cols.RemoveAt(ind);
        }
        return ConstCols;
    }

    public MatrixCol<T> GetColumnFail(string colName) => TryGetColumn(colName, out var col) ? col : throw new Exception($"Column '{colName}' was not found in the Matrix");
    public MatrixCol<T>? GetColumn(string colName) => TryGetColumn(colName, out var col) ? col : default;


    //https://developer.mozilla.org/en-US/docs/Web/HTML/Element/table
    public string ToHtml(NHeader? css = null)
    {
        StringWriter sb = new();
        XTag tag = new(sb, css ?? new NHeader());
        ToHtml(tag);
        return sb.ToString();
    }

    abstract public ITag ToHtml(XTag t);



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
        if (!Invariant) throw new InvalidOperationException("Matrix Invariant");

        MatrixCol<T>? levelCol = null;
        var lvlInd = Cols.FindIndex(c => c.Name == LevelField);
        if (lvlInd != -1)
        {
            levelCol = Cols[lvlInd];
            Cols.RemoveAt(lvlInd);
        }

        WriteRow(writer, Cols.Select(c => c.Name), null);
        for (int i = 0; i < RowsCount; ++i)
            WriteRow(writer, Cols.Select(c => c[i] ?? String.Empty), levelCol?[i]);
    }

    protected static void WriteRow(TextWriter writer, IEnumerable<string> cols, string? level)
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
