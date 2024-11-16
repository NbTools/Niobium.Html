namespace Niobium.Html;

public static class HtmlTag
{ 
    public static void CreateHtmlPage(string filename, HtmlParam htmlParams, Action<Tag> createContent)
    {
        string html = CreateHtmlPage(htmlParams, createContent);
        File.WriteAllText(filename, html);
    }

    public static string CreateHtmlPage(HtmlParam htmlParams, Action<Tag> createContent)
    {
        StringBuilder bld = new();
        if (createContent == null)
            throw new Exception("CreateContent action was not provided");

        //Run the body first to populate css with Styles provided while creating body
        NCss css = htmlParams.NCss ?? new NCss();
        Tag bodyTag = new(bld, css, null, 1);
        bodyTag.TT("body", t3 => createContent(t3));
        string bodyHtml = bld.ToString().TrimEnd();

        bld.Clear();
        bld.AppendLine("<!doctype html>");
        Tag htmlTag = new(bld, css);
        htmlTag.TT("html", t => t
            .TT("head", t1 =>
                {
                    if (!string.IsNullOrWhiteSpace(htmlParams.Title))
                        t1.TV("title", htmlParams.Title);
                    t1.TA("meta", a => a["charset"] = "utf-8");
                    t1.TA("meta", a => a["name", "viewport"]["content"] = "width=device-width, initial-scale=1.0");

                    if (htmlParams.DisableCache)
                    {
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

                    string? cssText = css.GetCss();

                    if (!string.IsNullOrEmpty(cssText))
                        t1.TT("style", t2 => t2.Html(cssText, closeOnNewLine: true));
                }
            //.TAV("script", a1 => a1["type", "text/javascript"]["language"] = "javascript", FileInOneLine(@"Data\JavaScript.js"), encode: false)
            )
            .Html(bodyHtml)
            //.TT("body", t2 => t2.TAT("div", a2 => a2["id"] = "content", t3 => createContent(t3))
            );
        return bld.ToString();
    }
}

/// <summary>
/// Timestamp is used for creating HTML with -N9UEsY ending to prevent problems with caching
/// </summary>
public record HtmlParam(string? Title, NCss? NCss = null, string? CssText = "default", string? CssFile = null, bool DisableCache = false);

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
