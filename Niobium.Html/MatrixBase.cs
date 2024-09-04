using System.Diagnostics.CodeAnalysis;

namespace Niobium.Html;

public class MatrixBase<T> where T : INamed
{
    public readonly List<T> Cols = [];

    public bool TryGetColumn(string colName, [NotNullWhen(true)] out T? col)
    {
        col = Cols.SingleOrDefault(c => c.Name == colName);
        return col != null;
    }

    public T GetColumn(string colName) => TryGetColumn(colName, out var col) ? col : throw new Exception($"Column '{colName}' was not found in the Matrix");
}
