namespace Niobium.Html;

/*public class StringMatrixCol(string name, int emptyCells) : INamed
{
    public string Name { get; init; } = name;
    private readonly List<string?> _Cells = [.. Enumerable.Repeat(String.Empty, emptyCells)];
    public bool IsHtml { get; set; } = false; //Encode by default

    public override string ToString() => $"{Name} {_Cells.Count}";

    public IReadOnlyList<string?> Cells => _Cells;
    public string? this[int ind] => _Cells[ind];
    public void Add(string? val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    /// <summary>
    /// Go throug the column and update the colums. Return null is update is not required
    /// </summary>
    /// <param name="updater">Updater function</param>
    public StringMatrixCol UpdateValues(Func<int, string?, string?> updater)
    {
        for (int i = 0; i < _Cells.Count; i++)
        {
            string? res = updater(i, _Cells[i]);
            if (res != null)
                _Cells[i] = res;
        }
        return this; //For chaining
    }

    /// <summary>
    /// Are column's values all the same?
    /// </summary>
    /// <returns></returns>
    public bool IsConst()
    {
        bool first = true;
        string? prev = null;
        foreach (string? val in _Cells)
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
            string? cell = _Cells[i];
            if (cell != null && cell.Length > symbolCount - 3)
                _Cells[i] = string.Concat(_Cells[i].AsSpan(0, symbolCount - 3), "...");
        }
    }

    public HashSet<string> ReplaceByDictInList(Dictionary<string, string> dict, char separator)
    {
        HashSet<string> nonResolved = [];

        for (int i = 0; i < _Cells.Count; i++)
        {
            string? val = _Cells[i];
            foreach (string listItem in val == null ? [] : val.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (dict.TryGetValue(listItem, out string? replacement))
                    val = val?.Replace(listItem, replacement);
                else
                    nonResolved.Add(listItem);
            }
            _Cells[i] = val;
        }
        return nonResolved;
    }
}*/