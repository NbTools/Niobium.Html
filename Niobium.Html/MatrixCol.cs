using System.Collections.ObjectModel;

namespace Niobium.Html;

public interface INamed
{
    string Name { get; }
}

public record MatrixCell<T>(string ColName, T Content, bool IsHtml) where T : class { }

public class MatrixCol : INamed
{
    public string Name { get; private set; }
    private readonly List<string> _Cells;
    public bool IsHtml = false; //Encode by default

    public MatrixCol(string name, int emptyCells)
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