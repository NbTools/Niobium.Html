namespace Niobium.Html;

public class HTag(TextWriter wrtr, NHeader? css = null, int level = 0) : XTag(wrtr, level)
{
    private readonly NHeader? Css = css;


}
