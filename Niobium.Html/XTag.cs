namespace Niobium.Html;

/// <summary>
/// Inteface that allows the caller to only create the Tag, so that the attributes always go first
/// </summary>
public interface ITag
{
    ITag T(string name, string text);
    ITag T(string name, Func<XTag, ITag>? sub = null);
    ITag Ts(string name, params Func<XTag, ITag>[] subs);
    ITag Empty();

#pragma warning disable IDE1006 // Naming Styles

    public ITag a(string href, string text);
    public ITag a(string href, Func<XTag, ITag> SubTags);
    public ITag a(string href, string clsName, Func<XTag, ITag> SubTags);
    public ITag a(string href, string clsName, string download, Func<XTag, ITag> SubTags);
    public ITag a(string href, NCssAttrib cls, Func<XTag, ITag> SubTags);
    public ITag a(string href, NCssAttrib cls, string download, Func<XTag, ITag> SubTags);

    public ITag img(string? src = null, string? cls = null);
    public ITag img(NCssAttrib? cls = null, string? src = null);

    public ITag br();

    public ITag div(Func<XTag, ITag> SubTags);
    public ITag div(string className, Func<XTag, ITag> SubTags);
    public ITag div(NCssAttrib cls, Func<XTag, ITag> SubTags);

    public ITag form(string url, Func<XTag, ITag> SubTags, string method = "post");

    public ITag input(InputType tp, string? name, string? val = null);
    public ITag inputWithLabel(InputType tp, string id, string label)
    {
        T("label", t => t["for", id].Text(label));
        return T(nameof(input), t => t["type", tp.ToString().Replace('_', '-')]["name", id]["id", id]);
    }

    public ITag p(Func<XTag, ITag> SubTags);
    public ITag h1(Func<XTag, ITag> SubTags);
    public ITag h2(Func<XTag, ITag> SubTags);
    public ITag h3(Func<XTag, ITag> SubTags);

    public ITag p(string text);
    public ITag h1(string text);
    public ITag h2(string text);
    public ITag h3(string text);

    public ITag span(string className, Func<XTag, ITag> SubTags);
    public ITag span(NCssAttrib cls, Func<XTag, ITag> SubTags);
    public ITag span(string? className = null, string? value = null);
    public ITag span(NCssAttrib cls, string? value = null);

    public ITag nav(Func<XTag, ITag> SubTags);
    public ITag ul(Func<XTag, ITag> SubTags);
    public ITag li(Func<XTag, ITag> SubTags);

    public ITag nav(string className, Func<XTag, ITag> SubTags);
    public ITag ul(string className, Func<XTag, ITag> SubTags);
    public ITag li(string className, Func<XTag, ITag> SubTags);

    public ITag nav(NCssAttrib cls, Func<XTag, ITag> SubTags);
    public ITag ul(NCssAttrib cls, Func<XTag, ITag> SubTags);
    public ITag li(NCssAttrib cls, Func<XTag, ITag> SubTags);

    public ITag script(Uri uri);
    public ITag script(string script);

#pragma warning restore IDE1006 // Naming Styles

    public enum InputType
    {
        button, checkbox, color, date, datetime_local, email, file, hidden, image, month,
        number, password, radio, range, reset, search, submit, tel, text, time, url,
    };
}

/// <summary>
/// Level can be overriden, if the generated XML is supposed to be inserted into already existing XML at certain level
/// </summary>
/// <param name="wrtr"></param>
/// <param name="level"></param>
public partial class XTag(TextWriter wrtr, int level = 0) : ITag
{
    enum TagContents { None = 0, Subtags = 1, Text = 2, MultilineText = 3 }

    private readonly TextWriter Wr = wrtr;
    private readonly Stack<TagContents> SubtagsStack = new();

    private const string IndentMin = "  ";
    private int Level = level;

    public XTag this[NCssAttrib? cls]
    {
        get
        {
            if (cls == null)
                return this;

            TryAdd(cls);
            return this["class", cls.Name.TrimStart('.')];
        }
    }

    private void SetSubtags(TagContents cont)
    {
        if (SubtagsStack.TryPop(out var _)) //If there is a head to set
            SubtagsStack.Push(cont);
    }

    public XTag this[string name, string? val]
    {
        get
        {
            if (val == null)
                return this;

            //TODO: handle empty val as attrib without value?
            Wr.Write($" {name}=\"{val}\"");
            return this;
        }
    }

    //public ITag Text(string? v, bool encode = true) => T("dummy", v ?? String.Empty); //TODO: do something about it

    public ITag Html(string? html) => Text(html, encode: false);

    public ITag Text(string? text, bool encode = true)
    {
        if (text == null)
            return this;

        bool newLine = text.Contains('\n');

        SetSubtags(newLine ? TagContents.MultilineText : TagContents.Text);
        Wr.Write('>'); //Close opening tag before text
        Wr.Write(encode ? System.Net.WebUtility.HtmlEncode(text) : text);
        return this; //TODO: think later - can't add tags after text ? or can we for mixed content?
    }

    public ITag T(string name, string text)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            CloseAndIndent();

        Wr.Write($"<{name}>");
        Wr.Write(text);
        Wr.Write($"</{name}");

        if (SubtagsStack.Count == 0)
            Wr.Write(">"); //The end of the file

        return this;
    }


    public ITag T(string name, Func<XTag, ITag>? Sub = null)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            CloseAndIndent(); //Indentation for the openind tag

        Wr.Write($"<{name}");

        if (Sub != null)
        {
            SubtagsStack.Push(TagContents.None); //Assume no subtags, otherwise will be overwritten in sub(this) call
            Level++;
            var _ = Sub(this);
            Level--;

            //After attributes and child tags were written
            ClosingTagChoice(name);
        }
        else
            Wr.Write('/');

        if (SubtagsStack.Count == 0) //One final bracket, because the tags do not close themselves and are closed by the following tag in indentation code.
            Wr.Write(">"); //The end of the file

        return this;
    }

    public ITag Ts(string name, params Func<XTag, ITag>[] subs)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            CloseAndIndent(); //Indentation for the openind tag

        Wr.Write($"<{name}");

        if (subs?.Length > 0)
        {
            Level++;
            foreach (var Sub in subs)
            {
                SubtagsStack.Push(TagContents.None); //Push for compatibility, we know there will be tags with Length > 0
                var _ = Sub(this);
                SubtagsStack.Pop(); //We don't need to know, because attributes are not possible
            }
            Level--;

            CloseAndIndent(); //Indentation for the closing tag, where will always be subtags here
            ClosingTag(name);
        }
        else
            Wr.Write('/');

        if (SubtagsStack.Count == 0)
            Wr.Write(">"); //The end of the file

        return this;
    }


    private void ClosingTagChoice(string name)
    {
        //After attributes and child tags were written
        TagContents SubTags = SubtagsStack.Pop();
        switch (SubTags)
        {
            case TagContents.None:
                Wr.Write('/');
                break;

            case TagContents.Subtags:
                CloseAndIndent(); //Indentation for the closing tag
                ClosingTag(name);
                break;

            case TagContents.Text:
                ClosingTag(name); //Closing tag for the text on the same line, no new line or indent 
                break;

            case TagContents.MultilineText:
                CloseAndIndent(close: false);
                ClosingTag(name); //Closing tag on the new line with indent 
                break;

            default:
                throw new InvalidDataException($"Unsupported TagContents: {SubTags}");
        }
    }


    public ITag Empty() => this;

    private void CloseAndIndent(bool close = true)
    {
        if (close)
            Wr.WriteLine('>');
        for (int i = Level; i > 0; --i) //level % 4
            Wr.Write(IndentMin);
    }

    private void ClosingTag(string closingTagName)
    {
        Wr.Write("</");
        Wr.Write(closingTagName);
    }
}
