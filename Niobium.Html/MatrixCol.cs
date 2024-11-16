using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public interface INamed
{
    string Name { get; }
}

public record MatrixCell<T>(string ColName, T Content, bool IsHtml = false) where T : class { }

public class MatrixCol<T>(string name, int emptyCells) : INamed
{
    public string Name { get; } = name;
    private readonly List<T?> _Cells = [.. Enumerable.Repeat<T?>(default, emptyCells)];
    public bool IsHtml = false; //Encode by default

    public override string ToString() => $"{Name} {_Cells.Count}";

    public IReadOnlyList<T?> Cells => _Cells; //TODO: think about making private
    public void Add(T? val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    public T? GetField(int ind) => _Cells[ind];
    public string? this[int ind]
    {
        get
        {
            T? val = _Cells[ind];
            return val switch
            {
                null => null,
                string str => str,
                JProperty jp => jp.Value.ToString(),
                _ => val.ToString(),
            };
        }
    }


    /// <summary>
    /// Go throug the column and update the colums. Return null is update is not required
    /// </summary>
    /// <param name="updater">Updater function</param>
    public MatrixCol<T> UpdateValues(Func<int, T?, T?> updater)
    {
        for (int i = 0; i < _Cells.Count; i++)
        {
            T? res = updater(i, _Cells[i]);
            if (res != null)
                _Cells[i] = res;
        }
        return this;
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

    public MatrixCol<T> SetHtml(bool isHtml = true) //Useful for chaining
    {
        IsHtml = isHtml;
        return this;
    }
}