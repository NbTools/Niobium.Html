using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public class JsonMatrix(string[]? predefinedColumns = null, string[]? ignoreColumns = null, Func<object, string?>? converter = null,
    Stack<string>? parentPropNames = null, HtmlInterceptor<string?>? htmlInterceptor = null) 
    : MatrixBase<JProperty>(predefinedColumns, ignoreColumns, converter, parentPropNames, htmlInterceptor)
{
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

    #region Html Functionality
    public void ToHtml(Tag t, Func<JsonMatrix, int, bool> filter) => t.TT("table", t1 =>
    {
        t.TT("tr", HtmlHeaders);
        for (int i = 0; i < RowsCount; i++)
        {
            if (filter?.Invoke(this, i) == true)
                t.TT("tr", t2 => HtmlRow(t2, i));
        }
    }); //thead tbody

    public override void ToHtml(Tag t)
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
                            t2.TV("td", String.Empty);
                    });
                }
            });
        }
    }


    private void HtmlHeaders(Tag t)
    {
        foreach (MatrixCol<JProperty> col in Cols)
            t.TV("th", col.Name);
    }

    private void HtmlRow(Tag t, int rowNum)
    {
        foreach (MatrixCol<JProperty> col in Cols)
        {
            JToken? val = col.Cells[rowNum]?.Value;
            if (val != null)
            {
                JsonObject jObj = new();
                t.TT("td", t2 => jObj.Convert(val, t));  //TODO: support handler and names
            }
            else
                t.TV("td", String.Empty);
        }
    }
    #endregion Html Functionality
}


