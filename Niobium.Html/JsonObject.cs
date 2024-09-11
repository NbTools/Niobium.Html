using Newtonsoft.Json.Linq;

namespace Niobium.Html;

public delegate bool HtmlInterceptor<T>(Stack<string> propNames, T propValue, Tag tag);

public class JsonObject(HtmlInterceptor<string?>? HtmlInterceptor = null, Stack<string>? parentPropNames = null)
{
    private readonly Stack<string> ParentPropNames = parentPropNames ?? new Stack<string>();

    public static string CreateHtml(string header, Action<Tag> tag) => HtmlTag.CreateHtmlPage(new HtmlParam(header), tag);

    public void Convert(JToken json, Tag tag)
    {
        if (json is JObject jobj)
        {
            if (!HandleMongoObjects(jobj, tag))
                tag.TT("table", table =>
                {
                    foreach (JToken item in jobj.Children())
                    {
                        if (item is JProperty jprop)
                        {
                            table.TT("tr", tr =>
                            {
                                tr.TV("th", jprop.Name);
                                ParentPropNames.Push(jprop.Name);
                                try
                                {
                                    JsonObject subObj = new(HtmlInterceptor, ParentPropNames);
                                    table.TT("td", td => subObj.Convert(jprop.Value, td));
                                }
                                finally { ParentPropNames.Pop(); }
                            });//Recursive
                        }
                        else
                            throw new Exception($"Only JProperties are supported inside JProperty. The child type: {item.GetType().Name}");
                    }
                });
        }
        else if (json is JValue jval)
        {
            if (!HandleSpecialValues(jval, tag))
            {
                string? str = jval.Value?.ToString();
                if (!HtmlInterceptor?.Invoke(ParentPropNames, str, tag) ?? true)
                    tag.Text(str ?? "NULL");
            }
        }
        else if (json is JArray jarr)
        {
            if (jarr.Children().Count() == 1)
            {
                JsonObject subObj = new(HtmlInterceptor, ParentPropNames);
                subObj.Convert(jarr.Children().First(), tag); //Do not nest if only one cell is in the array
            }
            else
            {
                if (jarr.Children().All(ch => ch.GetType().Name == nameof(JObject)))
                {   //Only handle the arrays of object with the matrix
                    JsonMatrix nbMatrix = new();
                    nbMatrix.AddJArray(jarr);
                    nbMatrix.ToHtml(tag);
                }
                else
                    tag.TT("table", table =>
                    {
                        foreach (JToken jtok in jarr.Children())
                        {
                            table.TT("tr", tr => tr.TT("td", td =>
                            {
                                JsonObject subObj = new(HtmlInterceptor, ParentPropNames);
                                subObj.Convert(jtok, td);
                            })); //Recursive
                        }
                    });
            }
        }
        else
            tag.p($"Unsupported JToken type: {json.GetType().Name}");
    }

    private static bool HandleSpecialValues(JValue jval, Tag tag)
    {
        if (jval.Value is { } vl && vl.ToString() is string str)
        {
            if (str.StartsWith("data:image"))
            {
                tag.TA("img", a => a["src", str]["height"] = "200");
                return true;
            }
        }
        return false;
    }

    private bool HandleMongoObjects(JObject jobj, Tag tag)
    {
        if (jobj.Children().Count() != 1)
            return false;

        if (jobj.Children().First() is not JProperty prop)
            return false;

        switch (prop.Name)
        {
            case "$numberInt":
            case "$numberLong":
            case "$oid":
                Convert(prop.Value, tag); //Recursive - skip inside
                return true;

            case "$date":
                if (prop.Value is JObject longObj && longObj.Children().Count() == 1 &&
                    longObj.First is JProperty longProp && longProp.Name == "$numberLong"
                    && long.TryParse(longProp.Value.ToString(), out long unixLong))
                {
                    DateTimeOffset dt = DateTimeOffset.FromUnixTimeMilliseconds(unixLong);
                    tag.Text(dt.ToLocalTime().ToString());
                    return true;
                }
                else
                    return false;

            default:
                return false;
        }
    }
}
