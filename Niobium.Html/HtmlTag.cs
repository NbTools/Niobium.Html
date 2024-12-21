namespace Niobium.Html;

public static class HtmlTag
{
    public static async Task<string> HtmlPage2String(HtmlParam htmlParams, Func<XTag, ITag> createContent)
    {
        using StringWriter respStream = new();
        await StreamHtmlPage(respStream, htmlParams, createContent);
        return respStream.ToString();
    }

    public static async Task StreamHtmlPage(TextWriter bld, HtmlParam htmlParams, Func<XTag, ITag> createContent)
    {
        //We have to write body to string because it creates styles while processing
        StringWriter bldBody = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        //Run the body first to populate css with Styles provided while creating body
        NHeader css = htmlParams.NHeader ?? new NHeader();
        XTag bodyTag = new(bldBody, css, 1); //, null, 1
        var _ = bodyTag.T("zzz", createContent);
        string bodyHtml = bldBody.ToString().Replace("<zzz>", String.Empty).Replace("</zzz>", String.Empty).TrimEnd(' ');  //TODO: deal later with the fake extra tag

        await bld.WriteLineAsync("<!doctype html>");
        await bld.WriteLineAsync();//TODO: remove later
        XTag htmlTag = new(bld, css);
        htmlTag.T("html", t =>
        {
            t.HtmlHeader(htmlParams, css).T("body", t => t.Text(bodyHtml, encode: false)); //Tag is required to provide inner html properly
            foreach (string src in css.GetScriptSources())
                t.script(src);
            return t;
        });
    }

    static ITag HtmlHeader(this XTag t0, HtmlParam htmlParams, NHeader css) => t0.T("head", t =>
    {
        if (!string.IsNullOrWhiteSpace(htmlParams.Title))
            t.T("title", htmlParams.Title);
        t.T("meta", a => a["charset", "utf-8"].Empty());
        t.T("meta", a => a["name", "viewport"]["content", "width=device-width, initial-scale=1.0"].Empty());

        if (htmlParams.DisableCache)
        {
            t.T("meta", a => a["http-equiv", "Cache-Control"]["content", "no-cache, no-store, must-revalidate"].Empty());
            t.T("meta", a => a["http-equiv", "Pragma"]["content", "no-cache"].Empty());
            t.T("meta", a => a["http-equiv", "Expires"]["content", "0"].Empty());
        }

        foreach (Uri url in css.GetScriptUris())
            t.script(url);

        if (!string.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
            t.T("link", a => a["rel", "stylesheet"]["href", htmlParams.CssFile].Empty());
        else
        {
            string? cssText = css.GetCss();
            if (!string.IsNullOrEmpty(cssText))
                t.T("style", t2 => t2.Text(cssText, encode: false));  //was closeOnNewLine: true
        }
        return t0;
        /*if (!String.IsNullOrEmpty(htmlParams.MbCssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
            t1.TA("link", a => a["rel", "stylesheet"]
            ["media", "only screen and (-moz-min-device-pixel-ratio: 2), only screen and (-o-min-device-pixel-ratio: 2/1), only screen and (-webkit-min-device-pixel-ratio: 2), only screen and (min-device-pixel-ratio: 2)"]
            ["href"] = htmlParams.MbCssFile);*/
    });
    //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)
}

/// <summary>
/// Timestamp is used for creating HTML with -N9UEsY ending to prevent problems with caching
/// </summary>
public record HtmlParam(string? Title, NHeader? NHeader = null, string? CssText = "default", string? CssFile = null, bool DisableCache = false);

public record HtmlFileName(string? Directory, string? Id, DateTime TimeStamp)
{
    public string HtmlFileJustName => $"{Id}{(TimeStamp == default ? String.Empty : "-" + TimeStamp.ToString("yyyyMMdd-HHmmss"))}.html";
    public string HtmlFileFullName => $"{Directory}\\{HtmlFileJustName}";
}

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
