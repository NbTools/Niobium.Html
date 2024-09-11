using Niobium.Html.Properties;

namespace Niobium.Html;

public class HtmlTag(StringBuilder aWr) : Tag(aWr, null)
{
    public static void CreateHtmlPage(string filename, HtmlParam htmlParams, Action<Tag> createContent)
    {
        string html = CreateHtmlPage(htmlParams, createContent);
        File.WriteAllText(filename, html);
    }

    public static async void CreateHtmlPage(string filename, HtmlParam htmlParams, Func<Tag, Task> createContent)
    {
        string html = await CreateHtmlPage(htmlParams, createContent);
        File.WriteAllText(filename, html);
    }

    //TODO: remove duplication (see below)
    public static Task<string> CreateHtmlPage(HtmlParam htmlParams, Func<Tag, Task> createContent)
    {
        StringBuilder bld = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        bld.AppendLine("<!doctype html>");
        Tag myT = Create(bld).TT("html", (Action<Tag>)(t => t
            .TT("head", (Action<Tag>)(t1 =>
            {
                if (!string.IsNullOrWhiteSpace(htmlParams.Title))
                    t1.TV("title", htmlParams.Title);
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

                if (!string.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                    t1.TA("link", a => a["rel", "stylesheet"]["href"] = htmlParams.CssFile);

                /*if (!String.IsNullOrEmpty(htmlParams.MbCssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                    t1.TA("link", a => a["rel", "stylesheet"]
                    ["media", "only screen and (-moz-min-device-pixel-ratio: 2), only screen and (-o-min-device-pixel-ratio: 2/1), only screen and (-webkit-min-device-pixel-ratio: 2), only screen and (min-device-pixel-ratio: 2)"]
                    ["href"] = htmlParams.MbCssFile);*/

                string? nbCss = htmlParams.NbCss?.GetCss();
                if (nbCss is null && !string.IsNullOrEmpty(htmlParams?.CssText))
                    nbCss = htmlParams.CssText == "default" ? new NbCss().GetCss() : null;

                if (!string.IsNullOrEmpty(nbCss))
                    t1.TV("style", nbCss, encode: false);
            }))
            //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)

            .TT("body", async t3 => await createContent(t3)))
            //.TT("body", t2 => t2.TAT("div", a2 => a2["id"] = "content", t3 => createContent(t3))
            );
        return Task.FromResult(bld.ToString());
    }

    public static string CreateHtmlPage(HtmlParam htmlParams, Action<Tag> createContent)
    {
        StringBuilder bld = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        bld.AppendLine("<!doctype html>");
        Tag myT = Create(bld).TT("html", (Action<Tag>)(t => t
            .TT("head", (Action<Tag>)(t1 =>
            {
                if (!string.IsNullOrWhiteSpace(htmlParams.Title))
                    t1.TV("title", htmlParams.Title);
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

                if (!string.IsNullOrEmpty(htmlParams.CssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                    t1.TA("link", a => a["rel", "stylesheet"]["href"] = htmlParams.CssFile);

                /*if (!String.IsNullOrEmpty(htmlParams.MbCssFile))  //<link rel="stylesheet" href="/lib/w3schools32.css">
                    t1.TA("link", a => a["rel", "stylesheet"]
                    ["media", "only screen and (-moz-min-device-pixel-ratio: 2), only screen and (-o-min-device-pixel-ratio: 2/1), only screen and (-webkit-min-device-pixel-ratio: 2), only screen and (min-device-pixel-ratio: 2)"]
                    ["href"] = htmlParams.MbCssFile);*/

                string? nbCss = htmlParams.NbCss?.GetCss();
                if (nbCss is null && !string.IsNullOrEmpty(htmlParams?.CssText))
                    nbCss = htmlParams.CssText == "default" ? new NbCss().GetCss() : null;

                if (!string.IsNullOrEmpty(nbCss))
                    t1.TV("style", nbCss, encode: false);
            })
                //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)
                )
            .TT("body", t3 => createContent(t3)))
            //.TT("body", t2 => t2.TAT("div", a2 => a2["id"] = "content", t3 => createContent(t3))
            );
        return bld.ToString();
    }
}

/// <summary>
/// Timestamp is used for creating HTML with -N9UEsY ending to prevent problems with caching
/// </summary>
public record HtmlParam(string? Title, NbCss? NbCss = null, string? CssText = "default", string? CssFile = null, bool DisableCache = false);

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
