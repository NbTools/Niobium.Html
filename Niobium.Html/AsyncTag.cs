namespace Niobium.Html;

/// <summary>
/// Tag based on StreamWriter for asynchronous response
/// </summary>
/// <param name="aWr"></param>
/// <param name="css"></param>
/// <param name="nameSpace"></param>
/// <param name="level"></param>
public class AsyncTag(TextWriter aWr, NHeader? css = null, string? nameSpace = null, int level = 0)
{
    private const string IndentMin = "  ";
    private int Level = level;
    private int TagCount = 0;
    private readonly TextWriter Wr = aWr;
    private readonly NHeader Css = css ?? new NHeader();
    private readonly string? NameSpace = nameSpace;

    private async Task<AsyncTag> CreateTag(int aLevel, string tagName, Action<AsyncTag>? Attribs, Action<AsyncTag>? suSTags, string? val, bool encode = true)
    {
        if (tagName.Contains('<') || tagName.Contains('>'))
            throw new ArgumentException("Illegal tagName " + tagName);

        Level = aLevel;
        Indentation(Level);

        TagCount++;
        await Wr.WriteAsync('<');
        if (!String.IsNullOrEmpty(NameSpace))
        {
            await Wr.WriteAsync(NameSpace);
            await Wr.WriteAsync(':');
        }
        await Wr.WriteAsync(tagName);
        Attribs?.Invoke(this);

        if (suSTags != null)
        {   //Opening a closing tags
            await Wr.WriteAsync('>');

            if (!String.IsNullOrEmpty(val))
                throw new ArgumentException("Both suSTags and the value provided for tag '{tagName}'");

            int tagsBefore = TagCount;
            Level++;
            suSTags(this);  //TODO: this can't return parameters because return parameter is used for chaining
            Level--;        //New lines could be made a responsibility of the code in suSTags

            if (TagCount > tagsBefore) //SuSTags were created, closing tag is on a new line
                Indentation(Level);
            ClosingTag(tagName);
        }
        else if (val != null) //Empty string should create opening and closing tags
        {
            await Wr.WriteAsync('>');
            await Wr.WriteAsync(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
            ClosingTag(tagName);
        }
        else
        { //Single tag
            await Wr.WriteAsync("/>");
        }
        return this;
    }

    private void ClosingTag(string tagName)
    {
        Wr.WriteAsync("</");
        if (!String.IsNullOrEmpty(NameSpace))
        {
            Wr.WriteAsync(NameSpace);
            Wr.WriteAsync(':');
        }
        Wr.WriteAsync(tagName);
        Wr.WriteAsync('>');
    }

    private void Indentation(int level)
    {
        Wr.WriteLineAsync();
        for (int i = level; i > 0; --i) //level % 4
            Wr.WriteAsync(IndentMin);
    }

    public async Task<AsyncTag> Attrib(string attName, string attValue) //Async indexers Don't work
    {
        await Wr.WriteAsync(' ');
        await Wr.WriteAsync(attName);
        await Wr.WriteAsync("=\"");
        await Wr.WriteAsync(attValue);
        await Wr.WriteAsync('\"');
        return this;
    }

    public Task<AsyncTag> TA(string tagName, Action<AsyncTag> Attribs) => CreateTag(Level, tagName, Attribs, null, null);
    public Task<AsyncTag> TT(string tagName, Action<AsyncTag> suSTags) => CreateTag(Level, tagName, null, suSTags, null);
    public Task<AsyncTag> TV(string tagName, string? val, bool encode = true) => CreateTag(Level, tagName, null, null, val, encode);

    public Task<AsyncTag> TAT(string tagName, Action<AsyncTag> attribs, Action<AsyncTag> suSTags) => CreateTag(Level, tagName, attribs, suSTags, null);
    public Task<AsyncTag> TAV(string tagName, Action<AsyncTag> attribs, string? val, bool encode = true) => CreateTag(Level, tagName, attribs, null, val, encode);

    public Task<AsyncTag> T(string tagName) => CreateTag(Level, tagName, null, null, null);

    public async Task<AsyncTag> Text(string? val, bool encode = true)
    {
        if (val != null)
            await Wr.WriteAsync(encode ? System.Net.WebUtility.HtmlEncode(val) : val);
        return this;
    }

    public async Task<AsyncTag> Html(string html, bool closeOnNewLine = false)
    {
        await Wr.WriteAsync(html);
        if (closeOnNewLine)
            TagCount++; //Pretend we've created a tag to force closing tag on the new line
        return this;
    }
}