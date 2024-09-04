using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class JsonMatrixCol(string name, int emptyCells) : INamed
{
    public string Name { get; } = name;
    private readonly List<JProperty?> _Cells = [.. Enumerable.Repeat<JProperty?>(null, emptyCells)];
    public bool IsHtml = false; //Encode by default

    public override string ToString() => $"{Name} {_Cells.Count}";

    public List<JProperty?> Cells => _Cells; //TODO: think about making private
    public JProperty? this[int ind] => _Cells[ind];
    public void Add(JProperty? val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    /// <summary>
    /// Go throug the column and update the colums. Return null is update is not required
    /// </summary>
    /// <param name="updater">Updater function</param>
    public void UpdateValues(Func<int, JProperty?, JProperty?> updater)
    {
        for (int i = 0; i < _Cells.Count; i++)
        {
            JProperty? res = updater(i, _Cells[i]);
            if (res != null)
                _Cells[i] = res;
        }
    }

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