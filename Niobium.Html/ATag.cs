using System.Net;

namespace Niobium.Html.Async;

/// <summary>
/// Inteface that allows the caller to only create the Tag, so that the attributes always go first
/// </summary>
public interface IATag
{
    Task<IATag> T(string name, string text, bool encode = true);
    Task<IATag> T(string name, Func<ATag, Task<IATag>>? sub = null);
    Task<IATag> Ts(string name, params Func<IATag, Task<IATag>>[] subs);
    Task<IATag> Empty();
}

public static class TagExt
{
    public static async Task<IATag> T(this Task<IATag> Caller, string name) => await (await Caller).T(name);
    public static async Task<IATag> T(this Task<ATag>  Caller, string name) => await (await Caller).T(name);

    public static async Task<IATag> T(this Task<IATag> Caller, string name, string text, bool encode = true) => await (await Caller).T(name, text, encode);
    public static async Task<IATag> T(this Task<ATag>  Caller, string name, string text, bool encode = true) => await (await Caller).T(name, text, encode);

    public static async Task<IATag> T(this Task<IATag> Caller, string name, Func<ATag, Task<IATag>>? sub = null) => await (await Caller).T(name, sub);
    public static async Task<IATag> T(this Task<ATag>  Caller, string name, Func<ATag, Task<IATag>>? sub = null) => await (await Caller).T(name, sub);

    public static async Task<IATag> Ts(this Task<IATag> Caller, string name, params Func<IATag, Task<IATag>>[] subs) => await (await Caller).Ts(name, subs);
    public static async Task<IATag> Ts(this Task<ATag>  Caller, string name, params Func<IATag, Task<IATag>>[] subs) => await (await Caller).Ts(name, subs);

    public static async Task<IATag> Empty(this Task<IATag> Caller) => await (await Caller).Empty();
    public static async Task<IATag> Empty(this Task<ATag>  Caller) => await (await Caller).Empty();

    public static async Task<IATag> Text(this Task<ATag> Caller, string? text, bool encode = true) => await (await Caller).Text(text, encode);

    public static async Task<ATag> A(this Task<ATag> Caller, string name, string? val) => await (await Caller).A(name, val);
}

/// <summary>
/// Level can be overriden, if the generated XML is supposed to be inserted into already existing XML at certain level
/// </summary>
/// <param name="wrtr"></param>
/// <param name="level"></param>
public class ATag(TextWriter wrtr, int level = 0) : IATag
{
    enum TagContents { None = 0, Subtags = 1, Text = 2, MultilineText = 3 }

    private readonly TextWriter Wr = wrtr;
    private readonly Stack<TagContents> SubtagsStack = new();

    private const string IndentMin = "  ";
    private int Level = level;

    public Task<ATag> Cls(NCssAttrib? cls) =>
        cls == null ? Task.FromResult(this) : A("class", cls.Name.TrimStart('.'));

    public async Task<ATag> A(string name, string? val)
    {
        if (val == null)
            return this;

        //TODO: handle empty val as attrib without value?
        await Wr.WriteAsync($" {name}=\"{val}\"");
        return this;
    }


#pragma warning disable IDE1006 // Naming Styles
    public Task<IATag> script(Uri uri) => T(nameof(script), t => A("src", uri.OriginalString).Text("")); //Needs closing tag
    public Task<IATag> script(string script) => T(nameof(script), t => A("type", "text/javascript").Text(script, encode: false));
#pragma warning restore IDE1006 // Naming Styles

    //public IATag Text(string? v, bool encode = true) => T("dummy", v ?? String.Empty); //TODO: do something about it

    public Task<IATag> Html(string? html) => Text(html, encode: false);

    public async Task<IATag> Text(string? text, bool encode = true)
    {
        if (text == null)
            return this;

        bool newLine = text.Contains('\n');

        SetSubtags(newLine ? TagContents.MultilineText : TagContents.Text);
        await Wr.WriteAsync('>'); //Close opening tag before text
        await Wr.WriteAsync(encode ? System.Net.WebUtility.HtmlEncode(text) : text);
        return this; //TODO: think later - can't add tags after text ? or can we for mixed content?
    }

    public async Task<IATag> T(string name, string text, bool encode = true)
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


    public async Task<IATag> T(string name, Func<ATag, Task<IATag>>? Sub = null)
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

            //After attributes and child tags were written
            await ClosingTagChoice(name);
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

    public async Task<IATag> Ts(string name, params Func<IATag, Task<IATag>>[] Subs)
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

            default:
                throw new InvalidDataException($"Unsupported TagContents: {SubTags}");
        }
    }


    public Task<IATag> Empty() => Task.FromResult<IATag>(this);

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
