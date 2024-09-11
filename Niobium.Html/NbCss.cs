namespace Niobium.Html;

public record NbCss(Dictionary<string, NbCssTag> Tags)
{
    public NbCss() : this(new Dictionary<string, NbCssTag>()) { }
    public NbCss(IEnumerable<NbCssTag> tags) : this(tags.ToDictionary(t => t.Name)) { }
    public NbCss(params NbCssTag[] tags) : this(tags.ToDictionary(t => t.Name)) { }

    private static readonly NbCssTag body = new(nameof(body), new() {
        { "background-color", "white" },
        { "font-family", @"""Segoe UI"",Arial,Helvetica,sans-serif" },
        { "font-size", "12px" },
        { "margin", "4px" },
    });

    private static readonly NbCssTag table_th_td = new("table, th, td", new() {
        { "border", "1px solid black" },
        { "border-collapse", "collapse" },
        { "text-align", "left" },
        { "padding", "2px" },
    });

    public string GetCss()
    {
        StringBuilder sb = new(Environment.NewLine);

        Tags.TryAdd(body.Name, body);
        Tags.TryAdd(table_th_td.Name, table_th_td);

        foreach ((string key, NbCssTag tag) in Tags.OrderBy(p => p.Key.TrimStart('.')))
        {
            tag.Print(sb);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public record NbCssTag(string Name, Dictionary<string, string> Attributes)
{
    public NbCssTag(string Name) : this(Name, []) { }

    public NbCssTag this[string name, string val]
    {
        get
        {
            Attributes.Add(name, val);
            return this;
        }
    }

    /*public NbCssTag Atrib(string name, string value)
    {
        Attributes.Add(name, value);
        return this;
    }*/

    public void Print(StringBuilder sb)
    {
        sb.Append(Name).AppendLine(" {");
        foreach ((string key, string val) in Attributes.OrderBy(p => p.Key))
            sb.Append('\t').Append(key).Append(": ").Append(val).AppendLine(";");

        sb.AppendLine("}");
    }
}