
namespace Niobium.Html.Async;

public interface IHtmlTag : IATag
{
#pragma warning disable IDE1006 // Naming Styles
    public ValueTask<IATag> a(string href, string text) =>
        T(nameof(a), t => t.A("href", href).Text(text));

    public ValueTask<IATag> a(string href, Func<IATag, ValueTask<IATag>> SubTags) => 
        T(nameof(a), async t => await SubTags(await t.A("href", href)));

    public ValueTask<IATag> a(string href, string className, Func<IATag, ValueTask<IATag>> SubTags) => 
        T(nameof(a), async t => await SubTags(await t.A("href", href).A("class", className)));

    public ValueTask<IATag> a(string href, string className, string download, Func<IATag, ValueTask<IATag>> SubTags) =>
        T(nameof(a), async t => await SubTags(await t.A("href", href).A("class", className).A("download", download)));

    public ValueTask<IATag> a(string href, NCssAttrib cls, Func<IATag, ValueTask<IATag>> SubTags) =>
        T(nameof(a), async t => await SubTags(await t.A("href", href).A("class", cls.Name)));

    public ValueTask<IATag> a(string href, NCssAttrib cls, string download, Func<IATag, ValueTask<IATag>> SubTags) =>
        T(nameof(a), async t => await SubTags(await t.A("href", href).A("class", cls.Name).A("download", download)));
#pragma warning restore IDE1006 // Naming Styles
}

public class AHtmlTag : ATag, IHtmlTag
{
    public AHtmlTag(TextWriter wrtr, int level = 0) : base(wrtr, level)
    {
    }
}


/*public partial class ATag : IATag
{
    private readonly NHeader NHeader = null!;

    public ATag(TextWriter wrtr, NHeader nHeader, int level = 0) : this(wrtr, level)
    {
        NHeader = nHeader;
    }

    public ValueTask<ATag> Cls(NCssAttrib? cls)
    {
        if (cls == null)
            return Task.FromResult(this);

        NHeader.TryAdd(cls);
        return A("class", cls.Name.TrimStart('.'));
    }


#pragma warning disable IDE1006 // Naming Styles
    public Task<IATag> script(Uri uri) => T(nameof(script), t => A("src", uri.OriginalString).Text("")); //Needs closing tag
    public Task<IATag> script(string script) => T(nameof(script), t => A("type", "text/javascript").Text(script, encode: false));
#pragma warning restore IDE1006 // Naming Styles


    public static async Task<string> HtmlPage2StringAsync(HtmlParam htmlParams, Func<ATag, Task<IATag>> createContent)
    {
        try
        {
            await using StringWriter respStream = new();
            await StreamHtmlPageAsync(respStream, htmlParams, createContent);
            return respStream.ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static async Task StreamHtmlPageAsync(TextWriter bld, HtmlParam htmlParams, Func<ATag, Task<IATag>> createContent)
    {
        //We have to write body to string because it creates styles while processing
        StringWriter bldBody = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        //Run the body first to populate css with Styles provided while creating body
        NHeader css = htmlParams.NHeader ?? new NHeader();
        ATag bodyTag = new(bldBody, css, 1); //, null, 1
        var _ = await bodyTag.T("zzz", async t => await createContent(t));
        string bodyHtml = bldBody.ToString().Replace("<zzz>", String.Empty).Replace("</zzz>", String.Empty);  //TODO: deal later with the fake extra tag

        await bld.WriteLineAsync("<!doctype html>");
        await bld.WriteLineAsync();//TODO: remove later
        ATag htmlTag = new(bld, css);
        await htmlTag.T("html", async t =>
        {
            await HtmlHeader(t, htmlParams, css);
            await t.T("body", bodyHtml, encode: false); //Tag is required to provide inner html properly
            return t;
        });
    }

    static Task<IATag> HtmlHeader(ATag t0, HtmlParam htmlParams, NHeader css) => t0.T("head", async t =>
    {
        if (!string.IsNullOrWhiteSpace(htmlParams.Title))
            await t.T("title", htmlParams.Title);
        await t.T("meta", a => a.A("charset", "utf-8").Empty());
        await t.T("meta", a => a.A("name", "viewport").A("content", "width=device-width, initial-scale=1.0").Empty());

        if (htmlParams.DisableCache)
        {
            await t.T("meta", a => a.A("http-equiv", "Cache-Control").A("content", "no-cache, no-store, must-revalidate").Empty());
            await t.T("meta", a => a.A("http-equiv", "Pragma").A("content", "no-cache").Empty());
            await t.T("meta", a => a.A("http-equiv", "Expires").A("content", "0").Empty());
        }

        foreach (Uri url in css.GetScriptUris())
            await t.script(url);

        if (!string.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
            await t.T("link", a => a.A("rel", "stylesheet").A("href", htmlParams.CssFile).Empty());
        else
        {
            string? cssText = css.GetCss();
            if (!string.IsNullOrEmpty(cssText))
                await t.T("style", t2 => t2.Text(cssText, encode: false));  //was closeOnNewLine: true
        }
        return t;
    });

}*/
