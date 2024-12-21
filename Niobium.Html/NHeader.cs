using System.Text.RegularExpressions;
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

namespace Niobium.Html;

public record NHeader(Dictionary<string, NCssAttrib> Tags, string? CssText = null)
{
    private readonly Lazy<HashSet<Uri>> ScriptUris = new();
    private readonly Lazy<List<string>> ScriptSrcs = new();

    public void AddScriptUri(Uri uri) => ScriptUris.Value.Add(uri);
    public void AddScript(string src) => ScriptSrcs.Value.Add(src);

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

        //Default Css
        Tags.TryAdd(body.Name, body);
        Tags.TryAdd(table_th_td.Name, table_th_td);

        foreach ((string key, NCssAttrib tag) in Tags.OrderBy(p => p.Key.TrimStart('.')))
        {
            tag.Print(sb);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd() + Environment.NewLine; //TODO: more elegant trimming?
    }

    public IEnumerable<Uri> GetScriptUris() => ScriptUris.IsValueCreated ? ScriptUris.Value.OrderBy(u => u.OriginalString) : [];
    public IEnumerable<string> GetScriptSources() => ScriptSrcs.IsValueCreated ? ScriptSrcs.Value : [];

    public bool TryAdd(NCssAttrib cls)
    {
        return Tags.TryAdd(cls.Name, cls);

    }
}

public record NCssAttrib(string Name, Dictionary<string, string> Attributes, NCssAttrib[] Children)
{
    public NCssAttrib(string Name) : this(Name, new Dictionary<string, string>(), []) { }
    public NCssAttrib(string Name, IEnumerable<(string key, string val)> attribs) : this(Name, attribs.ToDictionary(p => p.key, p => p.val), []) { }
    public NCssAttrib(string Name, IEnumerable<(string key, string val)> attribs, NCssAttrib[] children)
        : this(Name, attribs.ToDictionary(p => p.key, p => p.val), children) { }

    public string GetName() => Name; //TODO: count references here
    public override string ToString() => throw new NotImplementedException(nameof(ToString)); //Temporary to make sure only GetName is called

    public NCssAttrib this[string name, string val]
    {
        get
        {
            Attributes.Add(name, val);
            return this;
        }
    }

    private static readonly Regex cssOuter = new(@"^(.*)\{(.*)\}$");
    private static readonly Regex cssInner = new(@"[^;]+(?=;[^;]*)");

    public static NCssAttrib Parse(string text)
    {
        text = text.Trim().Replace("\r\n", "");
        Match match = cssOuter.Match(text);
        if (!match.Success)
            throw new SmartException($"Can't parse Css: {text}");

        string name = match.Groups[1].Value.Trim();
        string body = match.Groups[2].Value;
        MatchCollection matches = cssInner.Matches(body);
        if (matches.Count == 0)
            throw new SmartException($"Can't parse Css: {text}");

        IEnumerable<(string, string)> pairs = matches.Select(m =>
        {
            string txt = m.Value;
            int ind = txt.IndexOf(':');
            if (ind == -1)
                throw new SmartException($"Can't find ':' in {text}");

            return (txt[..ind].Trim(), txt[(ind+1)..]);
        });

        return new NCssAttrib(name, pairs);
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

    internal bool AddTo(NHeader css)
    {
        bool res = css.TryAdd(this);
        foreach (var child in Children)
            child.AddTo(css);
        return res;
    }
}