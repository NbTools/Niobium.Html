namespace Niobium.Html.Test;

public class AsyncTagTest
{
    [Fact]
    public async Task Test1()
    {
        using StringWriter wrtr = new();

        AsyncTag fTag = new(wrtr);
        AsyncTag resFTag = await fTag.TAT("tagname", 
            async a => await a.Attrib("attribute", "attribute_value"),
            async t => await t.TV("subtag", "SomeText") );

        var stringRes = wrtr.ToString();
        Assert.NotEmpty(stringRes);
    }
}