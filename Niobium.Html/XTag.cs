namespace Niobium.Html;

/// <summary>
/// Interface that allows the coller to create the Attributes and the tags
/// </summary>
public interface IAttr : ITag
{
    IAttr this[string name, string val] { get; }
    ITag Text(string? v, bool encode = true); //Return nothing (Task only)
}

/// <summary>
/// Inteface that allows the caller to only create the Tag, so that the attributes always go first
/// </summary>
public interface ITag
{
    ITag T(string name, string text);
    ITag T(string name, Func<IAttr, ITag>? sub = null);
    ITag T(string name, params Func<ITag, ITag>[] subs);
    ITag Empty();
}

/// <summary>
/// Level can be overriden, if the generated XML is supposed to be inserted into already existing XML at certain level
/// </summary>
/// <param name="wrtr"></param>
/// <param name="level"></param>
public class XTag(TextWriter wrtr, int level = 0) : IAttr
{
    enum TagContents { None = 0, Subtags = 1, Text = 2, MultilineText = 3 }

    private readonly TextWriter Wr = wrtr;
    private readonly Stack<TagContents> SubtagsStack = new();

    private const string IndentMin = "  ";
    private int Level = level;

    private void SetSubtags(TagContents cont)
    {
        if (SubtagsStack.TryPop(out var _)) //If there is a head to set
            SubtagsStack.Push(cont);
    }

    public IAttr this[string name, string val]
    {
        get
        {
            Wr.Write($" {name}=\"{val}\"");
            return this;
        }
    }

    //public ITag Text(string? v, bool encode = true) => T("dummy", v ?? String.Empty); //TODO: do something about it

    public ITag Text(string text, bool encode = true)
    {
        int newLine = text.IndexOf('\n');

        SetSubtags(newLine >= 0 ? TagContents.MultilineText : TagContents.Text);
        Wr.Write('>'); //Close opening tag before text
        Wr.Write(text); //TODO: encode
        return (ITag)null; //TODO: think later
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


    public ITag T(string name, Func<IAttr, ITag>? Sub = null)
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
        else
            Wr.Write('/');

        if (SubtagsStack.Count == 0) //One final bracket, because the tags do not close themselves and are closed by the following tag in indentation code.
            Wr.Write(">"); //The end of the file

        return this;
    }

    public ITag T(string name, params Func<ITag, ITag>[] subs)
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
