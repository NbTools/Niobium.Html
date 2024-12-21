using System.Net;

namespace Niobium.Html.Async;

/// <summary>
/// Inteface that allows the caller to only create the Tag, so that the attributes always go first
/// </summary>
public partial interface IATag
{
    ValueTask<IATag> T(string name, string text, bool encode = true);
    ValueTask<IATag> T(string name, Func<ATag, ValueTask<IATag>>? sub = null);
    ValueTask<IATag> Ts(string name, params Func<IATag, ValueTask<IATag>>[] subs);
    ValueTask<IATag> Text(string? text, bool encode = true);
    ValueTask<IATag> Empty();
}

public static class TagExt
{
    public static async ValueTask<IATag> T(this ValueTask<IATag> Caller, string name) => await (await Caller).T(name);
    public static async ValueTask<IATag> T(this ValueTask<ATag> Caller, string name) => await (await Caller).T(name);

    public static async ValueTask<IATag> T(this ValueTask<IATag> Caller, string name, string text, bool encode = true) => await (await Caller).T(name, text, encode);
    public static async ValueTask<IATag> T(this ValueTask<ATag> Caller, string name, string text, bool encode = true) => await (await Caller).T(name, text, encode);

    public static async ValueTask<IATag> T(this ValueTask<IATag> Caller, string name, Func<ATag, ValueTask<IATag>>? sub = null) => await (await Caller).T(name, sub);
    public static async ValueTask<IATag> T(this ValueTask<ATag> Caller, string name, Func<ATag, ValueTask<IATag>>? sub = null) => await (await Caller).T(name, sub);

    public static async ValueTask<IATag> Ts(this ValueTask<IATag> Caller, string name, params Func<IATag, ValueTask<IATag>>[] subs) => await (await Caller).Ts(name, subs);
    public static async ValueTask<IATag> Ts(this ValueTask<ATag> Caller, string name, params Func<IATag, ValueTask<IATag>>[] subs) => await (await Caller).Ts(name, subs);

    public static async ValueTask<IATag> Empty(this ValueTask<IATag> Caller) => await (await Caller).Empty();
    public static async ValueTask<IATag> Empty(this ValueTask<ATag> Caller) => await (await Caller).Empty();

    public static async ValueTask<IATag> Text(this ValueTask<IATag> Caller, string? text, bool encode = true) => await (await Caller).Text(text, encode);
    public static async ValueTask<IATag> Text(this ValueTask<ATag> Caller, string? text, bool encode = true) => await (await Caller).Text(text, encode);

    public static async ValueTask<ATag> A(this ValueTask<ATag> Caller, string name, string? val) => await (await Caller).A(name, val);
}

/// <summary>
/// Level can be overridden, if the generated XML is supposed to be inserted into already existing XML at certain level
/// </summary>
/// <param name="wrtr"></param>
/// <param name="level"></param>
public partial class ATag : IATag
{
    enum TagContents { None = 0, Subtags = 1, Text = 2, MultilineText = 3 }

    private readonly TextWriter Wr;
    private readonly Stack<TagContents> SubtagsStack;

    private const string IndentMin = "  ";
    private int Level;

    public ATag(TextWriter wrtr, int level = 0)
    {
        Wr = wrtr;
        SubtagsStack = new();
        Level = level;
    }

    public async ValueTask<ATag> A(string name, string? val)
    {
        if (val == null)
            return this;

        //TODO: handle empty val as attrib without value?
        await Wr.WriteAsync($" {name}=\"{val}\"");
        return this;
    }

    public ValueTask<IATag> Html(string? html) => Text(html, encode: false);

    public async ValueTask<IATag> Text(string? text, bool encode = true)
    {
        if (text == null)
            return this;

        bool newLine = text.Contains('\n');

        SetSubtags(newLine ? TagContents.MultilineText : TagContents.Text);
        await Wr.WriteAsync('>'); //Close opening tag before text
        await Wr.WriteAsync(encode ? System.Net.WebUtility.HtmlEncode(text) : text);
        return this; //TODO: think later - can't add tags after text ? or can we for mixed content?
    }

    public async ValueTask<IATag> T(string name, string text, bool encode = true)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            await CloseAndIndent();

        await Wr.WriteAsync($"<{name}>");
        await Wr.WriteAsync(encode ? WebUtility.HtmlEncode(text) : text);
        await Wr.WriteAsync($"</{name}");

        if (SubtagsStack.Count == 0)
            await Wr.WriteAsync(">"); //The end of the file

        return this;
    }


    public async ValueTask<IATag> T(string name, Func<ATag, ValueTask<IATag>>? Sub = null)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            await CloseAndIndent(); //Indentation for the openind tag

        await Wr.WriteAsync($"<{name}");

        if (Sub != null)
        {
            SubtagsStack.Push(TagContents.None); //Assume no subtags, otherwise will be overwritten in sub(this) call
            Level++;
            IATag res = await Sub(this);
            Assert(res);
            Level--;

            await ClosingTagChoice(name); //After attributes and child tags were written
        }
        else
            await Wr.WriteAsync('/');

        if (SubtagsStack.Count == 0) //One final bracket, because the tags do not close themselves and are closed by the following tag in indentation code.
            await Wr.WriteAsync(">"); //The end of the file

        return this;
    }

    private static void Assert(IATag? tag)
    {
        if (tag == null)
            throw new Exception("Tag is null");
    }

    public async ValueTask<IATag> Ts(string name, params Func<IATag, ValueTask<IATag>>[] Subs)
    {
        SetSubtags(TagContents.Subtags); //SubTags = true; //Set on the recursive call

        if (SubtagsStack.Count > 0)
            await CloseAndIndent(); //Indentation for the openind tag

        await Wr.WriteAsync($"<{name}");

        if (Subs?.Length > 0)
        {
            Level++;
            foreach (var Sub in Subs)
            {
                SubtagsStack.Push(TagContents.None); //Push for compatibility, we know there will be tags with Length > 0
                IATag res = await Sub(this);
                Assert(res);
                SubtagsStack.Pop(); //We don't need to know, because attributes are not possible
            }
            Level--;

            await CloseAndIndent(); //Indentation for the closing tag, where will always be subtags here
            await ClosingTag(name);
        }
        else
            await Wr.WriteAsync('/');

        if (SubtagsStack.Count == 0)
            await Wr.WriteAsync(">"); //The end of the file

        return this;
    }

    private void SetSubtags(TagContents cont)
    {
        if (SubtagsStack.TryPop(out var _)) //If there is a head to set
            SubtagsStack.Push(cont);
    }

    private async Task ClosingTagChoice(string name)
    {
        //After attributes and child tags were written
        TagContents SubTags = SubtagsStack.Pop();
        switch (SubTags)
        {
            case TagContents.None:
                await Wr.WriteAsync('/');
                break;

            case TagContents.Subtags:
                await CloseAndIndent(); //Indentation for the closing tag
                await ClosingTag(name);
                break;

            case TagContents.Text:
                await ClosingTag(name); //Closing tag for the text on the same line, no new line or indent 
                break;

            case TagContents.MultilineText:
                await CloseAndIndent(close: false);
                await ClosingTag(name); //Closing tag on the new line with indent 
                break;

            //default:
            //    throw new InvalidDataException($"Unsupported TagContents: {SubTags}");
        }
    }

    public ValueTask<IATag> Empty() => ValueTask.FromResult<IATag>(this);

    private async Task CloseAndIndent(bool close = true)
    {
        if (close)
            await Wr.WriteLineAsync('>');
        for (int i = Level; i > 0; --i) //level % 4
            await Wr.WriteAsync(IndentMin);
    }

    private async Task ClosingTag(string closingTagName)
    {
        await Wr.WriteAsync("</");
        await Wr.WriteAsync(closingTagName);
    }
}
