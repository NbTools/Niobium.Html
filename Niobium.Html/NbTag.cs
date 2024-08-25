namespace Niobium.Html;

public class NbTag //: INbAtrrib
{
    private const string IndentMin = "  ";
    public int Level = 0;
    private readonly StringBuilder Wr;
    private readonly string? NameSpace;

    public static NbTag Create(StringBuilder wr, string? nameSpace = null) => new(wr, nameSpace);

    protected NbTag(StringBuilder aWr, string? nameSpace)
    {
        Wr = aWr;
        NameSpace = nameSpace;
    }

    private NbTag CreateTag(int aLevel, string tagName, Action<NbTag>? Attribs, Action<NbTag>? subTags, string? val, bool encode = true)
    {
        if (tagName.Contains('<') || tagName.Contains('>'))
            throw new ArgumentException("Illegal tagName " + tagName);

        Level = aLevel;
        Indentation(Wr, Level);

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
            Wr.AppendLine(">");

            if (!String.IsNullOrEmpty(val))
                throw new ArgumentException("Both subTags and the value provided for tag '{tagName}'");

            Level++;
            subTags(this);  //TODO: this can't return parameters because return parameter is used for chaining
            Level--;        //New lines could be made a responsibility of the code in subTags
            Indentation(Wr, Level);
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
            Wr.AppendLine("/>");
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
        Wr.AppendLine(">");
    }

    private static void Indentation(StringBuilder wr, int level)
    {
        /*for (int i = level >> 2; i > 0; --i)
            wr.Append('\t');    //Tabs instead of four spaces
        if (level % 2 != 0)
            wr.Append("  ");*/
        for (int i = level; i > 0; --i) //level % 4
            wr.Append(IndentMin);
    }

    public NbTag Attrib(string attName, string attValue)
    {
        Wr.Append(' ');
        Wr.Append(attName);
        Wr.Append("=\"");
        Wr.Append(attValue);
        Wr.Append('\"');
        return this;
    }

    public NbTag this[string attrName, string? attValue]
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

    public NbTag a(string href, string text) => TAV(nameof(a), t => t["href"] = href, text);

    public NbTag a(string href, Action<NbTag> subTags) => TAT(nameof(a), t => t["href"] = href, subTags);
    public NbTag a(string href, string cls, Action<NbTag> subTags) => TAT(nameof(a), t => t["href", href]["class"] = cls, subTags);
    public NbTag a(string href, string cls, string download, Action<NbTag> subTags) => TAT(nameof(a), t => t["href", href]["class", cls]["download"] = download, subTags);

    public NbTag br() => Html("<br/>");

    public NbTag div(Action<NbTag> subTags) => CreateTag(Level, nameof(div), null, subTags, null);
    public NbTag div(string cls, Action<NbTag> subTags) => TAT(nameof(div), t => t["class"] = cls, subTags);

    public NbTag form(string url, Action<NbTag> subTags, string method = "post") => TAT(nameof(form), t => t["action", url]["method"] = method, subTags);

    //public NbTag img(string src) => TA(nameof(img), t => t["src"] = src);
    public NbTag img(string? src = null, string? cls = null) => TA(nameof(img), t => t["src", src]["class"] = cls);

    public NbTag input(InputType tp, string? name, string? val = null) => TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", name]["value"] = val);
    public NbTag inputWithLabel(InputType tp, string id, string label)
    {
        TAV("label", t => t["for"] = id, label);
        return TA(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", id]["id"] = id);
    }

    public NbTag p(Action<NbTag> subTags) => CreateTag(Level, nameof(p), null, subTags, null);
    public NbTag h1(Action<NbTag> subTags) => CreateTag(Level, nameof(h1), null, subTags, null);
    public NbTag h2(Action<NbTag> subTags) => CreateTag(Level, nameof(h2), null, subTags, null);
    public NbTag h3(Action<NbTag> subTags) => CreateTag(Level, nameof(h3), null, subTags, null);

    public NbTag p(string text) => CreateTag(Level, nameof(p), null, null, text);
    public NbTag h1(string text) => CreateTag(Level, nameof(h1), null, null, text);
    public NbTag h2(string text) => CreateTag(Level, nameof(h2), null, null, text);
    public NbTag h3(string text) => CreateTag(Level, nameof(h3), null, null, text);

    public NbTag span(string cls, Action<NbTag> subTags) => TAT(nameof(span), t => t["class"] = cls, subTags);
    public NbTag span(string? cls = null, string? value = null) => TAV(nameof(span), t => t["class"] = cls, value);

#pragma warning restore IDE1006 // Naming Styles

    public NbTag TA(string tagName, Action<NbTag> Attribs) => CreateTag(Level, tagName, Attribs, null, null);
    public NbTag TT(string tagName, Action<NbTag> subTags) => CreateTag(Level, tagName, null, subTags, null);
    public NbTag TV(string tagName, string val, bool encode = true) => CreateTag(Level, tagName, null, null, val, encode);

    public NbTag TAT(string tagName, Action<NbTag> attribs, Action<NbTag> subTags) => CreateTag(Level, tagName, attribs, subTags, null);
    public NbTag TAV(string tagName, Action<NbTag> attribs, string? val, bool encode = true) => CreateTag(Level, tagName, attribs, null, val, encode);

    public NbTag Tag(string tagName) => CreateTag(Level, tagName, null, null, null);

    [Obsolete("Use Text instead")]
    public NbTag Value(string val, bool encode)
    {
        string encoded = encode ? System.Net.WebUtility.HtmlEncode(val) : val;
        Wr.Append(encoded);
        return this;
    }

    public NbTag Text(string val)
    {
        Wr.AppendLine(System.Net.WebUtility.HtmlEncode(val));
        return this;
    }

    public NbTag Html(string html)
    {
        Wr.AppendLine(html);
        return this;
    }

    /*public NbTag TTs(string tagName, IEnumerable<string> attValues)
    {
        if (attValues != null)
            foreach (var item in attValues)
            {
                CreateTag(Level, tagName, null, null, item);
            }
        return this;
    /

    public string this[string tagName]
    {
        set { CreateTag(Level, tagName, null, null, value); }
    }

    public NbTag this[string tagName, string attValue] => CreateTag(Level, tagName, null, null, attValue);*/
}

public class HtmlTag : NbTag
{
    public HtmlTag(StringBuilder aWr) : base(aWr, null) { }

    public static void CreateHtmlPage(string filename, HtmlParam htmlParams, Action<NbTag> createContent)
    {
        string html = CreateHtmlPage(htmlParams, createContent);
        File.WriteAllText(filename, html);
    }


    public static async void CreateHtmlPage(string filename, HtmlParam htmlParams, Func<NbTag, Task> createContent)
    {
        string html = await CreateHtmlPage(htmlParams, createContent);
        File.WriteAllText(filename, html);
    }

    public static Task<string> CreateHtmlPage(HtmlParam htmlParams, Func<NbTag, Task> createContent)
    {
        StringBuilder bld = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        bld.AppendLine("<!doctype html>");
        var myT = NbTag.Create(bld).TT("html", t => t
            .TAT("head", a1 => a1["title"] = htmlParams.Title ?? "Untitled",
                t1 =>
                {
                    t1.TA("meta", a => a["charset"] = "utf-8");
                    t1.TA("meta", a => a["name", "viewport"]["content"] = "width=device-width, initial-scale=1.0");

                    if (htmlParams.DisableCache)
                    {
                        /* 
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
<meta http-equiv="Pragma" content="no-cache" />
<meta http-equiv="Expires" content="0" />
                         */
                        t1.TA("meta", a => a["http-equiv", "Cache-Control"]["content"] = "no-cache, no-store, must-revalidate");
                        t1.TA("meta", a => a["http-equiv", "Pragma"]["content"] = "no-cache");
                        t1.TA("meta", a => a["http-equiv", "Expires"]["content"] = "0");
                    }

                    if (!String.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                        t1.TA("link", a => a["rel", "stylesheet"]["href"] = htmlParams.CssFile);

                    /*if (!String.IsNullOrEmpty(htmlParams.MbCssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                        t1.TA("link", a => a["rel", "stylesheet"]
                        ["media", "only screen and (-moz-min-device-pixel-ratio: 2), only screen and (-o-min-device-pixel-ratio: 2/1), only screen and (-webkit-min-device-pixel-ratio: 2), only screen and (min-device-pixel-ratio: 2)"]
                        ["href"] = htmlParams.MbCssFile);*/

                    if (!String.IsNullOrEmpty(htmlParams.CssText))
                        t1.TV("style", htmlParams.CssText, encode: false);
                }
                //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)
                )
            .TT("body", async t3 => await createContent(t3))
            //.TT("body", t2 => t2.TAT("div", a2 => a2["id"] = "content", t3 => createContent(t3))
            );
        return Task.FromResult(bld.ToString());
    }

    public static string CreateHtmlPage(HtmlParam htmlParams, Action<NbTag> createContent)
    {
        StringBuilder bld = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        bld.AppendLine("<!doctype html>");
        var myT = NbTag.Create(bld).TT("html", t => t
            .TAT("head", a1 => a1["title"] = htmlParams.Title ?? "Untitled",
                t1 =>
                {
                    t1.TA("meta", a => a["charset"] = "utf-8");
                    t1.TA("meta", a => a["name", "viewport"]["content"] = "width=device-width, initial-scale=1.0");

                    if (htmlParams.DisableCache)
                    {
                        /* 
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
<meta http-equiv="Pragma" content="no-cache" />
<meta http-equiv="Expires" content="0" />
                         */
                        t1.TA("meta", a => a["http-equiv", "Cache-Control"]["content"] = "no-cache, no-store, must-revalidate");
                        t1.TA("meta", a => a["http-equiv", "Pragma"]["content"] = "no-cache");
                        t1.TA("meta", a => a["http-equiv", "Expires"]["content"] = "0");
                    }

                    if (!String.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                        t1.TA("link", a => a["rel", "stylesheet"]["href"] = htmlParams.CssFile);

                    /*if (!String.IsNullOrEmpty(htmlParams.MbCssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                        t1.TA("link", a => a["rel", "stylesheet"]
                        ["media", "only screen and (-moz-min-device-pixel-ratio: 2), only screen and (-o-min-device-pixel-ratio: 2/1), only screen and (-webkit-min-device-pixel-ratio: 2), only screen and (min-device-pixel-ratio: 2)"]
                        ["href"] = htmlParams.MbCssFile);*/

                    if (!String.IsNullOrEmpty(htmlParams.CssText))
                        t1.TV("style", htmlParams.CssText, encode: false);
                }
                //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)
                )
            .TT("body", t3 => createContent(t3))
            //.TT("body", t2 => t2.TAT("div", a2 => a2["id"] = "content", t3 => createContent(t3))
            );
        return bld.ToString();
    }
}

/// <summary>
/// Timestamp is used for creating HTML with -N9UEsY ending to prevent problems with caching
/// </summary>
public record HtmlParam(string? Title, string? CssText = null, string? CssFile = null, bool DisableCache = false);

/*public record HtmlFileName(string? Directory, string? Id, DateTime TimeStamp)
{
    public string HtmlFileJustName => $"{Id}{(TimeStamp == default ? String.Empty : "-" + TimeStamp.ToNbase64())}.html";
    public string HtmlFileFullName => $"{Directory}\\{HtmlFileJustName}";
}*/

// Think about destination dir (for deployment) and subdir (within the site)


/*HTML Global Attributes https://www.w3schools.com/tags/ref_standardattributes.asp
accesskey Specifies a shortcut key to activate/focus an element
class Specifies one or more classnames for an element(refers to a class in a style sheet)
contenteditable Specifies whether the content of an element is editable or not
data-* Used to store custom data private to the page or application
dir     Specifies the text direction for the content in an element
draggable Specifies whether an element is draggable or not
hidden  Specifies that an element is not yet, or is no longer, relevant
id Specifies a unique id for an element
lang Specifies the language of the element's content
spellcheck Specifies whether the element is to have its spelling and grammar checked or not
style Specifies an inline CSS style for an element

title Specifies extra information about an element
translate   Specifies whether the content of an element should be translated or not*/
