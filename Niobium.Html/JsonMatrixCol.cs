using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class JsonMatrixCol(string name, int emptyCells) : INamed
{
    public string Name { get; } = name;
    private readonly List<JProperty?> _Cells = [.. Enumerable.Repeat<JProperty?>(null, emptyCells)];
    public bool IsHtml = false; //Encode by default

    public override string ToString() => $"{Name} {_Cells.Count}";

    public ReadOnlyCollection<JProperty?> Cells => _Cells.AsReadOnly();
    public JProperty? this[int ind] => _Cells[ind];
    public void Add(JProperty? val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    public bool IsConst() //Are column's values all the same?
    {
        bool first = true;
        string? prev = null;
        foreach (string val in _Cells.Select(c => c?.ToString() ?? String.Empty))
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
}