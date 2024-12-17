namespace Niobium.Html.Test;

public class MatrixTest
{
    [Fact]
    public async Task HtmlInterceptor_Test()
    {
        const string csv = """
            Col1,Col2
            Val11,Val12
            Val21,Val22
            """;

        StringMatrix matrix = new(htmlInterceptor: Intercept);
        matrix.LoadCsv(csv);

        string html = await HtmlTag.HtmlPage2String(new HtmlParam("Header"), matrix.ToHtml);
        List<HtmlRow> rows = HtmlHelper.ParseTable(html);

        Assert.Equal("Val11", rows[1][0]);
        Assert.Equal("Overriden-Val12", rows[1][1]);
        Assert.Equal("Val21", rows[2][0]);
        Assert.Equal("Overriden-Val22", rows[2][1]);
    }

    private static bool Intercept(Stack<string> propName, string? propValue, XTag tag)
    {
        if (propName.Match("Col2"))
        {
            tag.Text("Overriden-" + propValue);
            return true;
        }
        return false;
    }

    [Fact]
    public void ManipulatingColums()
    {
        StringMatrix matrix = new(["Col1", "Col2"]);

        Assert.Null(matrix.GetColumn("NonExistant"));
        Assert.Throws<Exception>(() => matrix.GetColumnFail("NonExistant"));
        Assert.False(matrix.TryGetColumn("NonExistant", out var _));

        Assert.NotNull(matrix.GetColumn("Col1"));
        Assert.NotNull(matrix.GetColumnFail("Col1"));
        Assert.True(matrix.TryGetColumn("Col1", out var _));

        //matrix.GetColumnFail("Col1")?.IsHtml = true; //Doesn't compile
        matrix.GetColumnFail("Col1")?.SetHtml();
        Assert.True(matrix.GetColumnFail("Col1").IsHtml);
    }
}


