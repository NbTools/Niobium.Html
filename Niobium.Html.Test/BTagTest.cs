using System.Text;

namespace Niobium.Html.Test;

public class BTagTest
{
    [Fact]
    public void Test1()
    {
        StringBuilder bld = new();

        NTag t = new(bld);
        t.T("tagname", [("attribute", "attribute_value")],
            t => t.T("subtag", "SomeText"));

        var stringRes = bld.ToString().Trim();
        Assert.Equal(res1, stringRes);
    }

    static readonly string res1 = """
        <tagname attribute="attribute_value">
          <subtag>SomeText</subtag>
        </tagname>
        """.ReplaceLineEndings();
}
