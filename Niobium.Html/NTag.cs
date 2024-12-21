namespace Niobium.Html;
using AttValPairs = IEnumerable<(string attName, string? attValue)>;

/// <summary>
/// Tag based on StringBuilder and used for synchronouse content return
/// </summary>
/// <param name="Bld"></param>
/// <param name="Css"></param>
/// <param name="NameSpace"></param>
/// <param name="Level"></param>
public class NTag
{
    private int Level;
    private readonly StringBuilder Wr;
    private readonly NHeader Css;
    private readonly string? NameSpace;

    private const string IndentMin = "  ";
    private int TagCount = 0;

    public NTag(StringBuilder bld, NHeader? css = null, string? nameSpace = null, int level = 0)
    {
        Wr = bld;
        Css = css ?? new NHeader();
        NameSpace = nameSpace;
        Level = level;
    }

    private NTag CreateTag(int aLevel, string tagName, AttValPairs? attributes, Action<NTag>? subTags, string? val, bool encode = true)
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

        if (attributes != null)
        {
            foreach ((string attName, string? attValue) in attributes)
            {
                if (attValue != null)
                    Attrib(attName, attValue); //Invoke function is order to enable reference counting at source
            }
        }

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
        if (Wr.Length > 0) //Don't start resulting text with a new line
            Wr.AppendLine();
        for (int i = level; i > 0; --i) //level % 4
            Wr.Append(IndentMin);
    }

    public NTag Attrib(string attName, string attValue)
    {
        Wr.Append(' ');
        Wr.Append(attName);
        Wr.Append("=\"");
        Wr.Append(attValue);
        Wr.Append('\"');
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

    public NTag a(string href, string text) => T(nameof(a), [("href", href)], text);

    public NTag a(string href, Action<NTag> subTags) => T(nameof(a), [("href", href)], subTags);
    public NTag a(string href, string cls, Action<NTag> subTags) => T(nameof(a), [("href", href), ("class", cls)], subTags);
    public NTag a(string href, string cls, string download, Action<NTag> subTags) => T(nameof(a), [("href", href), ("class", cls), ("download", download)], subTags);
    public NTag a(string href, NCssAttrib cls, Action<NTag> subTags) => T(nameof(a), [("href", href), ("class", cls.Name)], subTags);
    public NTag a(string href, NCssAttrib cls, string download, Action<NTag> subTags) => T(nameof(a), [("href", href), ("class", cls.Name), ("download", download)], subTags);

    public NTag br() => Html("<br/>");

    public NTag div(Action<NTag> subTags) => CreateTag(Level, nameof(div), null, subTags, null);
    public NTag div(string cls, Action<NTag> subTags) => T(nameof(div), [("class", cls)], subTags);
    public NTag div(NCssAttrib cls, Action<NTag> subTags) => T(nameof(div), [("class", cls.Name)], subTags);

    public NTag form(string url, Action<NTag> subTags, string method = "post") => T(nameof(form), [("action", url), ("method", method)], subTags);

    public NTag img(string src, string cls) => T(nameof(img), [("src", src), ("class", cls)]);
    public NTag img(NCssAttrib cls) => T(nameof(img), [("class", cls.Name)]);
    public NTag img(NCssAttrib cls, string src) => T(nameof(img), [("src", src), ("class", cls.Name)]);

    public NTag input(InputType tp, string name, string val) => T(nameof(input), [("type", tp.ToString().Replace('_', '-')), ("name", name), ("value", val)]);
    public NTag inputWithLabel(InputType tp, string id, string label)
    {
        T("label", [("for", id)], label);
        return T(nameof(input), [("type", tp.ToString().Replace('_', '-')), ("name", id), ("id", id)]);
    }

    public NTag p(Action<NTag> subTags) => CreateTag(Level, nameof(p), null, subTags, null);
    public NTag h1(Action<NTag> subTags) => CreateTag(Level, nameof(h1), null, subTags, null);
    public NTag h2(Action<NTag> subTags) => CreateTag(Level, nameof(h2), null, subTags, null);
    public NTag h3(Action<NTag> subTags) => CreateTag(Level, nameof(h3), null, subTags, null);

    public NTag p(string text) => CreateTag(Level, nameof(p), null, null, text);
    public NTag h1(string text) => CreateTag(Level, nameof(h1), null, null, text);
    public NTag h2(string text) => CreateTag(Level, nameof(h2), null, null, text);
    public NTag h3(string text) => CreateTag(Level, nameof(h3), null, null, text);

    public NTag span(string cls, Action<NTag> subTags) => T(nameof(span), [("class", cls)], subTags);
    public NTag span(string cls, string? value = null) => T(nameof(span), [("class", cls)], value);
    public NTag span(NCssAttrib cls, Action<NTag> subTags) => T(nameof(span), [("class", cls.Name)], subTags);
    public NTag span(NCssAttrib cls, string? value = null) => T(nameof(span), [("class", cls.Name)], value);

    public NTag nav(Action<NTag> subTags) => CreateTag(Level, nameof(nav), null, subTags, null);
    public NTag nav(string cls, Action<NTag> subTags) => CreateTag(Level, nameof(nav), [("class", cls)], subTags, null);
    public NTag nav(NCssAttrib cls, Action<NTag> subTags) => CreateTag(Level, nameof(nav), [("class", cls.Name)], subTags, null);
    public NTag ul(Action<NTag> subTags) => CreateTag(Level, nameof(ul), null, subTags, null);
    public NTag ul(string cls, Action<NTag> subTags) => CreateTag(Level, nameof(ul), [("class", cls)], subTags, null);
    public NTag ul(NCssAttrib cls, Action<NTag> subTags) => CreateTag(Level, nameof(ul), [("class", cls.Name)], subTags, null);
    public NTag li(Action<NTag> subTags) => CreateTag(Level, nameof(li), null, subTags, null);
    public NTag li(string cls, Action<NTag> subTags) => CreateTag(Level, nameof(li), [("class", cls)], subTags, null);
    public NTag li(NCssAttrib cls, Action<NTag> subTags) => CreateTag(Level, nameof(li), [("class", cls.Name)], subTags, null);

#pragma warning restore IDE1006 // Naming Styles

    public NTag T(string tagName, AttValPairs Attribs) => CreateTag(Level, tagName, Attribs, null, null);
    public NTag T(string tagName, Action<NTag> subTags) => CreateTag(Level, tagName, null, subTags, null);
    public NTag T(string tagName, string? val, bool encode = true) => CreateTag(Level, tagName, null, null, val, encode);

    public NTag T(string tagName, AttValPairs attribs, Action<NTag> subTags) => CreateTag(Level, tagName, attribs, subTags, null);
    public NTag T(string tagName, AttValPairs attribs, string? val, bool encode = true) => CreateTag(Level, tagName, attribs, null, val, encode);

    public NTag T(string tagName) => CreateTag(Level, tagName, null, null, null);
    public NTag Ts(string tagName, params Action<NTag>[] subTags) => CreateTag(Level, tagName, null, t =>
    {
        foreach (Action<NTag> subTag in subTags)
            subTag(t);
    }, null);

    public NTag Text(string? val, bool encode = true)
    {
        if (val != null)
            Wr.Append(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
        return this;
    }

    public NTag Html(string html, bool closeOnNewLine = false)
    {
        Wr.Append(html);
        if (closeOnNewLine)
            TagCount++; //Pretend we've created a tag to force closing tag on the new line
        return this;
    }
}
