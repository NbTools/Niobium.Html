namespace Niobium.Html.Test;

public class BTagTest
{
    [Fact]
    public void Test1()
    {
        StringWriter bld = new();

        NTag t = new(bld);
        t.TAT("tagname",
            a => a.Attrib("attribute", "attribute_value"),
            t => t.TV("subtag", "SomeText"));

        var stringRes = bld.ToString().Trim();
        Assert.Equal(res1, stringRes);
    }

    const string res1 = @"<tagname attribute=""attribute_value"">
  <subtag>SomeText</subtag>
</tagname>";

}
