using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class JsonMatrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null, Func<object, string?>? converter = null,
    Stack<string>? parentPropNames = null, HtmlInterceptor<string?>? htmlInterceptor = null) 
    : MatrixBase<JProperty>(predefinedColumns, ignoreColumns, converter, parentPropNames, htmlInterceptor)
{
    public void AddJArray(JArray jarr) => AddJTokens(jarr);

    public void AddJTokens(IEnumerable<JToken> tokens)
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

    #region Html Functionality
    public void ToHtml(XTag t, Func<JsonMatrix, int, bool> filter) => t.T("table", t1 =>
    {
        t.T("tr", HtmlHeaders);
        for (int i = 0; i < RowsCount; i++)
        {
            if (filter?.Invoke(this, i) == true)
                t.T("tr", t2 => HtmlRow(t2, i));
        }
        return t1;
    }); //thead tbody

    public override ITag ToHtml(XTag t)
    {
        //The main table - just the headers and the rows
        t.T("table", t1 =>
        {
            t.T("tr", HtmlHeaders);
            for (int i = 0; i < RowsCount; i++)
                t.T("tr", t2 => HtmlRow(t2, i));
            return t1;
        });

        //Constant columns - a separate second table, only if ConstCols were calculated
        if (ConstCols.Count > 0)
        {
            t.T("p", " ");
            t.T("h2", "Constant Headers");
            t.T("table", t1 =>
            {
                t.T("tr", t2 => t2.T("th", "Name").T("th", "Value"));
                foreach ((string key, JProperty? val) in ConstCols.OrderBy(p => p.Key))
                {
                    t.T("tr", t2 =>
                    {
                        t2.T("td", key);
                        if (val != null)
                        {
                            ParentPropNames.Push(val.Name);
                            try
                            {
                                JsonObject jObj = new(HtmlInterceptor, ParentPropNames);
                                jObj.Convert(val, t2); //Recursive
                            }
                            finally { ParentPropNames.Pop(); }

                        }
                        else
                            t2.T("td", String.Empty);
                        return t2;
                    });
                }
                return t1;
            });
        }
        return t;
    }


    private ITag HtmlHeaders(XTag t)
    {
        foreach (MatrixCol<JProperty> col in Cols)
            t.T("th", col.Name);
        return t;
    }

    private ITag HtmlRow(XTag t, int rowNum)
    {
        foreach (MatrixCol<JProperty> col in Cols)
        {
            JToken? val = col.Cells[rowNum]?.Value;
            if (val != null)
            {
                ParentPropNames.Push(col.Name);
                try
                {
                    JsonObject jObj = new(HtmlInterceptor, ParentPropNames);
                    t.T("td", t2 => jObj.Convert(val, t));
                }
                finally { ParentPropNames.Pop(); }
            }
            else
                t.T("td", String.Empty);
        }
        return t;
    }
    #endregion Html Functionality
}


