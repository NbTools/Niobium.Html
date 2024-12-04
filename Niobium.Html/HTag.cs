namespace Niobium.Html;

public partial class XTag : IAttr //Partial class for now, but could possibly be made derived
{
    private readonly NHeader Css;
    public XTag(TextWriter wrtr, NHeader css, int level = 0) : this(wrtr, level) => Css = css;

    public bool TryAdd(NCssAttrib cls) => Css.TryAdd(cls);

    //protected override bool TryAdd(NCssAttrib cls) => Css.TryAdd(cls);

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable format

    public ITag a(string href, string text)                                                  => T(nameof(a), t => t["href", href].Text(text));
    public ITag a(string href, Func<IAttr, ITag> SubTags)                                    => T(nameof(a), t => SubTags(t["href", href]));
    public ITag a(string href, string className, Func<IAttr, ITag> SubTags)                  => T(nameof(a), t => SubTags(t["href", href]["class", className]));
    public ITag a(string href, string className, string download, Func<IAttr, ITag> SubTags) => T(nameof(a), t => SubTags(t["href", href]["class", className]["download", download]));
    public ITag a(string href, NCssAttrib cls, Func<IAttr, ITag> SubTags)                    => T(nameof(a), t => SubTags(t["href", href][cls]));
    public ITag a(string href, NCssAttrib cls, string download, Func<IAttr, ITag> SubTags)   => T(nameof(a), t => SubTags(t["href", href][cls]["download", download]));

    public ITag img(string? src = null, string? cls = null)     => T(nameof(img), t => t["src", src]["class", cls]);
    public ITag img(NCssAttrib? cls = null, string? src = null) => T(nameof(img), t => t["src", src][cls]);

    public ITag br() => Html("<br/>");

    public ITag div(Func<IAttr, ITag> SubTags) => T(nameof(div), SubTags);
    public ITag div(string className, Func<IAttr, ITag> SubTags) => T(nameof(div), t => SubTags(t["class", className]));
    public ITag div(NCssAttrib cls, Func<IAttr, ITag> SubTags) => T(nameof(div), t => SubTags(t[cls]));

    public ITag form(string url, Func<IAttr, ITag> SubTags, string method = "post") => T(nameof(form), t => SubTags(t["action", url]["method", method]));

    public ITag input(ITag.InputType tp, string? name, string? val = null) => T(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", name]["value", val]);
    public ITag inputWithLabel(ITag.InputType tp, string id, string label)
    {
        T("label", t => t["for", id].Text(label));
        return T(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", id]["id", id]);
    }

    public ITag p (Func<IAttr, ITag> SubTags) => T(nameof(p),  SubTags);
    public ITag h1(Func<IAttr, ITag> SubTags) => T(nameof(h1), SubTags);
    public ITag h2(Func<IAttr, ITag> SubTags) => T(nameof(h2), SubTags);
    public ITag h3(Func<IAttr, ITag> SubTags) => T(nameof(h3), SubTags);

    public ITag p (string text) => T(nameof(p),  t => t.Text(text));
    public ITag h1(string text) => T(nameof(h1), t => t.Text(text));
    public ITag h2(string text) => T(nameof(h2), t => t.Text(text));
    public ITag h3(string text) => T(nameof(h3), t => t.Text(text));

    public ITag span(string className, Func<IAttr, ITag> SubTags)    => T(nameof(span), t => SubTags(t["class", className]));
    public ITag span(NCssAttrib cls, Func<IAttr, ITag> SubTags)      => T(nameof(span), t => SubTags(t[cls]));
    public ITag span(string? className = null, string? value = null) => T(nameof(span), t => t["class", className].Text(value));
    public ITag span(NCssAttrib cls, string? value = null)           => T(nameof(span), t => t[cls].Text(value));

    public ITag nav(Func<IAttr, ITag> SubTags) => T(nameof(nav), SubTags);
    public ITag ul (Func<IAttr, ITag> SubTags) => T(nameof(ul), SubTags);
    public ITag li (Func<IAttr, ITag> SubTags) => T(nameof(li), SubTags);

    public ITag nav(string className, Func<IAttr, ITag> SubTags) => T(nameof(nav), t => SubTags(t["class", className]));
    public ITag ul (string className, Func<IAttr, ITag> SubTags) => T(nameof(ul),  t => SubTags(t["class", className]));
    public ITag li (string className, Func<IAttr, ITag> SubTags) => T(nameof(li),  t => SubTags(t["class", className]));

    public ITag nav(NCssAttrib cls, Func<IAttr, ITag> SubTags) => T(nameof(nav), t => SubTags(t[cls]));
    public ITag ul (NCssAttrib cls, Func<IAttr, ITag> SubTags) => T(nameof(ul),  t => SubTags(t[cls]));
    public ITag li (NCssAttrib cls, Func<IAttr, ITag> SubTags) => T(nameof(li),  t => SubTags(t[cls]));

    public ITag script(Uri uri) => T(nameof(script), t => t["src", uri.OriginalString].Text("")); //Needs closing tag
    public ITag script(string script) => T(nameof(script), t => t["type", "text/javascript"].Text(script, encode: false));

#pragma warning restore format
#pragma warning restore IDE1006 // Naming Styles
}
