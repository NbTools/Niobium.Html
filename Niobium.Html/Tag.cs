namespace Niobium.Html;

public class Tag(StringBuilder aWr, NCss css, string? nameSpace = null, int level = 0)
{
    private const string IndentMin = "  ";
    private int Level = level;
    private int TagCount = 0;
    private readonly StringBuilder Wr = aWr;
    private readonly NCss Css = css;
    private readonly string? NameSpace = nameSpace;

    private Tag CreateTag(int aLevel, string tagName, Action<Tag>? Attribs, Action<Tag>? subTags, string? val, bool encode = true)
    {
        if (tagName.Contains('<') || tagName.Contains('>'))
            throw new ArgumentException("Illegal tagName " + tagName);

        Level = aLevel;
        Indentation(Level);

        TagCount++;
        Wr.Append('<');
        if (!String.IsNullOrEmpty(NameSpace))
        {
            Wr.Append(NameSpace);
            Wr.Append(':');
        }
        Wr.Append(tagName);
        Attribs?.Invoke(this);

        if (subTags != null)
        {   //Opening a closing tags
            Wr.Append('>');

            if (!String.IsNullOrEmpty(val))
                throw new ArgumentException("Both subTags and the value provided for tag '{tagName}'");

            int tagsBefore = TagCount;
            Level++;
            subTags(this);  //TODO: this can't return parameters because return parameter is used for chaining
            Level--;        //New lines could be made a responsibility of the code in subTags

            if (TagCount > tagsBefore) //Subtags were created, closing tag is on a new line
                Indentation(Level);
            ClosingTag(tagName);
        }
        else if (val != null) //Empty string should create opening and closing tags
        {
            Wr.Append('>');
            Wr.Append(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
            ClosingTag(tagName);
        }
        else
        { //Single tag
            Wr.Append("/>");
        }
        return this;
    }

    private void ClosingTag(string tagName)
    {
        Wr.Append("</");
        if (!String.IsNullOrEmpty(NameSpace))
        {
            Wr.Append(NameSpace);
            Wr.Append(':');
        }
        Wr.Append(tagName);
        Wr.Append('>');
    }

    private void Indentation(int level)
    {
        Wr.AppendLine();
        for (int i = level; i > 0; --i) //level % 4
            Wr.Append(IndentMin);
    }

    private Tag Attrib(string attName, string attValue)
    {
        Wr.Append(' ');
        Wr.Append(attName);
        Wr.Append("=\"");
        Wr.Append(attValue);
        Wr.Append('\"');
        return this;
    }

    private Tag Cls(NCssAttrib? cls)
    {
        if (cls != null)
        {
            Css.TryAdd(cls);
            Attrib("class", cls.Name.TrimStart('.'));
        }
        return this;
    }

    public Tag this[string attrName, string? attValue]
    {
        get
        {
            if (attValue != null)
                Attrib(attrName, attValue);
            return this;
        }
    }

    public string? this[string attrName]
    {
        set
        {
            if (value != null)
                Attrib(attrName, value);
        }
    }

    public enum InputType
    {
        button,
        checkbox,
        color,
        date,
        datetime_local,
        email,
        file,
        hidden,
        image,
        month,
        number,
        password,
        radio,
        range,
        reset,
        search,
        submit,
        tel,
        text,
        time,
        url,
    };

#pragma warning disable IDE1006 // Naming Styles

    public Tag a(string href, string text) => TAV(nameof(a), t => t["href"] = href, text);

    public Tag a(string href, Action<Tag> subTags) => TAT(nameof(a), t => t["href"] = href, subTags);
    public Tag a(string href, string cls, Action<Tag> subTags) => TAT(nameof(a), t => t["href", href]["class"] = cls, subTags);
    public Tag a(string href, string cls, string download, Action<Tag> subTags) => TAT(nameof(a), t => t["href", href]["class", cls]["download"] = download, subTags);
    public Tag a(string href, NCssAttrib cls, Action<Tag> subTags) => TAT(nameof(a), t => t["href", href].Cls(cls), subTags);
    public Tag a(string href, NCssAttrib cls, string download, Action<Tag> subTags) => TAT(nameof(a), t => t["href", href].Cls(cls)["download"] = download, subTags);

    public Tag br() => Html("<br/>");

    public Tag div(Action<Tag> subTags) => CreateTag(Level, nameof(div), null, subTags, null);
    public Tag div(string cls, Action<Tag> subTags) => TAT(nameof(div), t => t["class"] = cls, subTags);
    public Tag div(NCssAttrib cls, Action<Tag> subTags) => TAT(nameof(div), t => t.Cls(cls), subTags);

    public Tag form(string url, Action<Tag> subTags, string method = "post") => TAT(nameof(form), t => t["action", url]["method"] = method, subTags);

    //public NbTag img(string src) => TA(nameof(img), t => t["src"] = src);
    public Tag img(string? src = null, string? cls = null) => TA(nameof(img), t => t["src", src]["class"] = cls);
    public Tag img(NCssAttrib cls) => TA(nameof(img), t => t.Cls(cls));
    public Tag img(NCssAttrib? cls = null, string? src = null) => TA(nameof(img), t => t["src", src].Cls(cls));

    public Tag input(InputType tp, string? name, string? val = null) => TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", name]["value"] = val);
    public Tag inputWithLabel(InputType tp, string id, string label)
    {
        TAV("label", t => t["for"] = id, label);
        return TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", id]["id"] = id);
    }

    public Tag p(Action<Tag> subTags) => CreateTag(Level, nameof(p), null, subTags, null);
    public Tag h1(Action<Tag> subTags) => CreateTag(Level, nameof(h1), null, subTags, null);
    public Tag h2(Action<Tag> subTags) => CreateTag(Level, nameof(h2), null, subTags, null);
    public Tag h3(Action<Tag> subTags) => CreateTag(Level, nameof(h3), null, subTags, null);

    public Tag p(string text) => CreateTag(Level, nameof(p), null, null, text);
    public Tag h1(string text) => CreateTag(Level, nameof(h1), null, null, text);
    public Tag h2(string text) => CreateTag(Level, nameof(h2), null, null, text);
    public Tag h3(string text) => CreateTag(Level, nameof(h3), null, null, text);

    public Tag span(string cls, Action<Tag> subTags) => TAT(nameof(span), t => t["class"] = cls, subTags);
    public Tag span(string? cls = null, string? value = null) => TAV(nameof(span), t => t["class"] = cls, value);
    public Tag span(NCssAttrib cls, Action<Tag> subTags) => TAT(nameof(span), t => t.Cls(cls), subTags);
    public Tag span(NCssAttrib cls, string? value = null) => TAV(nameof(span), t => t.Cls(cls), value);

    public Tag nav(Action<Tag> subTags) => CreateTag(Level, nameof(nav), null, subTags, null);
    public Tag nav(string cls, Action<Tag> subTags) => CreateTag(Level, nameof(nav), t => t["class"] = cls, subTags, null);
    public Tag nav(NCssAttrib cls, Action<Tag> subTags) => CreateTag(Level, nameof(nav), t => t.Cls(cls), subTags, null);
    public Tag ul(Action<Tag> subTags) => CreateTag(Level, nameof(ul), null, subTags, null);
    public Tag ul(string cls, Action<Tag> subTags) => CreateTag(Level, nameof(ul), t => t["class"] = cls, subTags, null);
    public Tag ul(NCssAttrib cls, Action<Tag> subTags) => CreateTag(Level, nameof(ul), t => t.Cls(cls), subTags, null);
    public Tag li(Action<Tag> subTags) => CreateTag(Level, nameof(li), null, subTags, null);
    public Tag li(string cls, Action<Tag> subTags) => CreateTag(Level, nameof(li), t => t["class"] = cls, subTags, null);
    public Tag li(NCssAttrib cls, Action<Tag> subTags) => CreateTag(Level, nameof(li), t => t.Cls(cls), subTags, null);

#pragma warning restore IDE1006 // Naming Styles

    public Tag TA(string tagName, Action<Tag> Attribs) => CreateTag(Level, tagName, Attribs, null, null);
    public Tag TT(string tagName, Action<Tag> subTags) => CreateTag(Level, tagName, null, subTags, null);
    public Tag TV(string tagName, string? val, bool encode = true) => CreateTag(Level, tagName, null, null, val, encode);

    public Tag TAT(string tagName, Action<Tag> attribs, Action<Tag> subTags) => CreateTag(Level, tagName, attribs, subTags, null);
    public Tag TAV(string tagName, Action<Tag> attribs, string? val, bool encode = true) => CreateTag(Level, tagName, attribs, null, val, encode);

    public Tag T(string tagName) => CreateTag(Level, tagName, null, null, null);

    public Tag Text(string? val, bool encode = true)
    {
        if (val != null)
            Wr.Append(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
        return this;
    }

    public Tag Html(string html, bool closeOnNewLine = false)
    {
        Wr.Append(html);
        if (closeOnNewLine)
            TagCount++; //Pretend we've created a tag to force closing tag on the new line
        return this;
    }
}
