namespace Niobium.Html;

public interface ITagOnly
{
    NTag TA(string tagName, Action<NTag> Attribs);
    NTag TT(string tagName, Action<ITagOnly> subTags);
    NTag TV(string tagName, string? val, bool encode = true);

    NTag TAT(string tagName, Action<NTag> attribs, Action<ITagOnly> subTags);
    NTag TAV(string tagName, Action<NTag> attribs, string? val, bool encode = true);

    NTag T(string tagName);
    NTag Text(string? val, bool encode = true);
    NTag Html(string html, bool closeOnNewLine = false);

#pragma warning disable IDE1006 // Naming Styles
    NTag img(string? src = null, string? cls = null);
    NTag img(NCssAttrib cls);
    NTag img(NCssAttrib? cls = null, string? src = null);
#pragma warning restore IDE1006 // Naming Styles
}

/// <summary>
/// Tag based on StringBuilder and used for synchronouse content return
/// </summary>
/// <param name="aWr"></param>
/// <param name="css"></param>
/// <param name="nameSpace"></param>
/// <param name="level"></param>
public class NTag(StringWriter aWr, NHeader? css = null, string? nameSpace = null, int level = 0) : ITagOnly
{
    private const string IndentMin = "  ";
    private int Level = level;
    private int TagCount = 0;
    private readonly StringWriter Wr = aWr;
    private readonly NHeader Css = css ?? new NHeader();
    private readonly string? NameSpace = nameSpace;

    private NTag CreateTag(int aLevel, string tagName, Action<NTag>? Attribs, Action<ITagOnly>? subTags, string? val, bool encode = true)
    {
        if (tagName.Contains('<') || tagName.Contains('>'))
            throw new ArgumentException("Illegal tagName " + tagName);

        Level = aLevel;
        Indentation(Level);

        TagCount++;
        Wr.Write('<');
        if (!String.IsNullOrEmpty(NameSpace))
        {
            Wr.Write(NameSpace);
            Wr.Write(':');
        }
        Wr.Write(tagName);
        Attribs?.Invoke(this);

        if (subTags != null)
        {   //Opening a closing tags
            Wr.Write('>');

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
            Wr.Write('>');
            Wr.Write(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
            ClosingTag(tagName);
        }
        else
        { //Single tag
            Wr.Write("/>");
        }
        return this;
    }

    private void ClosingTag(string tagName)
    {
        Wr.Write("</");
        if (!String.IsNullOrEmpty(NameSpace))
        {
            Wr.Write(NameSpace);
            Wr.Write(':');
        }
        Wr.Write(tagName);
        Wr.Write('>');
    }

    private void Indentation(int level)
    {
        Wr.WriteLine();
        for (int i = level; i > 0; --i) //level % 4
            Wr.Write(IndentMin);
    }

    public NTag Attrib(string attName, string attValue)
    {
        Wr.Write(' ');
        Wr.Write(attName);
        Wr.Write("=\"");
        Wr.Write(attValue);
        Wr.Write('\"');
        return this;
    }

    private NTag Cls(NCssAttrib? cls)
    {
        if (cls != null)
        {
            Css.TryAdd(cls);
            Attrib("class", cls.Name.TrimStart('.'));
        }
        return this;
    }

    public NTag this[string attrName, string? attValue]
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

    public NTag a(string href, string text) => TAV(nameof(a), t => t["href"] = href, text);

    public NTag a(string href, Action<ITagOnly> subTags) => TAT(nameof(a), t => t["href"] = href, subTags);
    public NTag a(string href, string cls, Action<ITagOnly> subTags) => TAT(nameof(a), t => t["href", href]["class"] = cls, subTags);
    public NTag a(string href, string cls, string download, Action<ITagOnly> subTags) => TAT(nameof(a), t => t["href", href]["class", cls]["download"] = download, subTags);
    public NTag a(string href, NCssAttrib cls, Action<ITagOnly> subTags) => TAT(nameof(a), t => t["href", href].Cls(cls), subTags);
    public NTag a(string href, NCssAttrib cls, string download, Action<ITagOnly> subTags) => TAT(nameof(a), t => t["href", href].Cls(cls)["download"] = download, subTags);

    public NTag br() => Html("<br/>");

    public NTag div(Action<ITagOnly> subTags) => CreateTag(Level, nameof(div), null, subTags, null);
    public NTag div(string cls, Action<ITagOnly> subTags) => TAT(nameof(div), t => t["class"] = cls, subTags);
    public NTag div(NCssAttrib cls, Action<ITagOnly> subTags) => TAT(nameof(div), t => t.Cls(cls), subTags);

    public NTag form(string url, Action<ITagOnly> subTags, string method = "post") => TAT(nameof(form), t => t["action", url]["method"] = method, subTags);

    public NTag img(string? src = null, string? cls = null) => TA(nameof(img), t => t["src", src]["class"] = cls);
    public NTag img(NCssAttrib cls) => TA(nameof(img), t => t.Cls(cls));
    public NTag img(NCssAttrib? cls = null, string? src = null) => TA(nameof(img), t => t["src", src].Cls(cls));

    public NTag input(InputType tp, string? name, string? val = null) => TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", name]["value"] = val);
    public NTag inputWithLabel(InputType tp, string id, string label)
    {
        TAV("label", t => t["for"] = id, label);
        return TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", id]["id"] = id);
    }

    public NTag p(Action<ITagOnly> subTags) => CreateTag(Level, nameof(p), null, subTags, null);
    public NTag h1(Action<ITagOnly> subTags) => CreateTag(Level, nameof(h1), null, subTags, null);
    public NTag h2(Action<ITagOnly> subTags) => CreateTag(Level, nameof(h2), null, subTags, null);
    public NTag h3(Action<ITagOnly> subTags) => CreateTag(Level, nameof(h3), null, subTags, null);

    public NTag p(string text) => CreateTag(Level, nameof(p), null, null, text);
    public NTag h1(string text) => CreateTag(Level, nameof(h1), null, null, text);
    public NTag h2(string text) => CreateTag(Level, nameof(h2), null, null, text);
    public NTag h3(string text) => CreateTag(Level, nameof(h3), null, null, text);

    public NTag span(string cls, Action<ITagOnly> subTags) => TAT(nameof(span), t => t["class"] = cls, subTags);
    public NTag span(string? cls = null, string? value = null) => TAV(nameof(span), t => t["class"] = cls, value);
    public NTag span(NCssAttrib cls, Action<ITagOnly> subTags) => TAT(nameof(span), t => t.Cls(cls), subTags);
    public NTag span(NCssAttrib cls, string? value = null) => TAV(nameof(span), t => t.Cls(cls), value);

    public NTag nav(Action<ITagOnly> subTags) => CreateTag(Level, nameof(nav), null, subTags, null);
    public NTag nav(string cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(nav), t => t["class"] = cls, subTags, null);
    public NTag nav(NCssAttrib cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(nav), t => t.Cls(cls), subTags, null);
    public NTag ul(Action<ITagOnly> subTags) => CreateTag(Level, nameof(ul), null, subTags, null);
    public NTag ul(string cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(ul), t => t["class"] = cls, subTags, null);
    public NTag ul(NCssAttrib cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(ul), t => t.Cls(cls), subTags, null);
    public NTag li(Action<ITagOnly> subTags) => CreateTag(Level, nameof(li), null, subTags, null);
    public NTag li(string cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(li), t => t["class"] = cls, subTags, null);
    public NTag li(NCssAttrib cls, Action<ITagOnly> subTags) => CreateTag(Level, nameof(li), t => t.Cls(cls), subTags, null);

#pragma warning restore IDE1006 // Naming Styles

    public NTag TA(string tagName, Action<NTag> Attribs) => CreateTag(Level, tagName, Attribs, null, null);
    public NTag TT(string tagName, Action<ITagOnly> subTags) => CreateTag(Level, tagName, null, subTags, null);
    public NTag TV(string tagName, string? val, bool encode = true) => CreateTag(Level, tagName, null, null, val, encode);

    public NTag TAT(string tagName, Action<NTag> attribs, Action<ITagOnly> subTags) => CreateTag(Level, tagName, attribs, subTags, null);
    public NTag TAV(string tagName, Action<NTag> attribs, string? val, bool encode = true) => CreateTag(Level, tagName, attribs, null, val, encode);

    public NTag T(string tagName) => CreateTag(Level, tagName, null, null, null);

    public NTag Text(string? val, bool encode = true)
    {
        if (val != null)
            Wr.Write(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
        return this;
    }

    public NTag Html(string html, bool closeOnNewLine = false)
    {
        Wr.Write(html);
        if (closeOnNewLine)
            TagCount++; //Pretend we've created a tag to force closing tag on the new line
        return this;
    }
}
