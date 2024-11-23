namespace Niobium.Html;

public record NHeader(Dictionary<string, NCssAttrib> Tags, string? CssText = null)
{
    public NHeader() : this(new Dictionary<string, NCssAttrib>()) { }
    public NHeader(string css) : this([], css) { }
    public NHeader(IEnumerable<NCssAttrib> tags) : this(tags.ToDictionary(t => t.Name)) { }
    public NHeader(params NCssAttrib[] tags) : this(tags.ToDictionary(t => t.Name)) { }

    private static readonly NCssAttrib body = new(nameof(body), [
        ( "background-color", "white" ),
        ( "font-family", @"""Segoe UI"",Arial,Helvetica,sans-serif" ),
        ( "font-size", "12px" ),
        ( "margin", "4px" ),
    ]);

    private static readonly NCssAttrib table_th_td = new("table, th, td", [
        ( "border", "1px solid black" ),
        ( "border-collapse", "collapse" ),
        ( "text-align", "left" ),
        ( "padding", "2px" ),
    ]);

    public string GetCss()
    {
        if (CssText != null)
            return CssText;

        StringBuilder sb = new(Environment.NewLine);

        Tags.TryAdd(body.Name, body);
        Tags.TryAdd(table_th_td.Name, table_th_td);

        foreach ((string key, NCssAttrib tag) in Tags.OrderBy(p => p.Key.TrimStart('.')))
        {
            tag.Print(sb);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd() + "\r\n"; //TODO: more elegant trimming?
    }

    public bool TryAdd(NCssAttrib cls) => Tags.TryAdd(cls.Name, cls);
}

public record NCssAttrib(string Name, Dictionary<string, string> Attributes)
{
    public NCssAttrib(string Name) : this(Name, new Dictionary<string, string>()) { }
    public NCssAttrib(string Name, IEnumerable<(string key, string val)> attribs) : this(Name, attribs.ToDictionary(p => p.key, p => p.val)) { }

    public NCssAttrib this[string name, string val]
    {
        get
        {
            Attributes.Add(name, val);
            return this;
        }
    }

#pragma warning disable IDE1006 // Naming Styles - mimich CSS for better readability
    public NCssAttrib background_image(FileInfo value)
    {
        string ext = value.Extension.TrimStart('.');
        string prefix = $"url(data:image/{ext};base64,";
        string res = prefix + Convert.ToBase64String(File.ReadAllBytes(value.FullName)) + ")";
        return background_image(res);
    }

    public NCssAttrib background_image(string value)
    {
        Attributes.Add("background-image", value.StartsWith("url") ? value : $"url({value})");
        return this;
    }
#pragma warning restore IDE1006 // Naming Styles


    public void Print(StringBuilder sb)
    {
        sb.Append(Name).AppendLine(" {");
        foreach ((string key, string val) in Attributes.OrderBy(p => p.Key))
            sb.Append('\t').Append(key).Append(": ").Append(val).AppendLine(";");

        sb.AppendLine("}");
    }
}