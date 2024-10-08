﻿using System.Collections.ObjectModel;

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
        _Cells = [.. Enumerable.Repeat(String.Empty, emptyCells)];
    }

    public override string ToString() => $"{Name} {_Cells.Count}";

    public List<string> Cells => _Cells; //TODO: think about making private
    public string this[int ind] => _Cells[ind];
    public void Add(string val) => _Cells.Add(val);
    public int Count => _Cells.Count;

    /// <summary>
    /// Go throug the column and update the colums. Return null is update is not required
    /// </summary>
    /// <param name="updater">Updater function</param>
    public MatrixCol UpdateValues(Func<int, string, string?> updater)
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
        HashSet<string> nonResolved = [];

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
        HashSet<string> nonResolved = [];

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