namespace Niobium.Html.Test;

public class XTagTest
{
#pragma warning disable IDE0039 //Function variables just for example

    private static readonly string fourTagsExample = """
    <root>
      <t1/>
      <t2 att1="attval"/>
      <t3>
        <subtag/>
      </t3>
      <t4 att1="attval">
        <subtag/>
      </t4>
    </root>
    """.ReplaceLineEndings();

    [Fact]
    public void FourCombinationsAsParams_Tags()
    {
        //Local functions
        static ITag F1(ITag t) => t.T("t1"); //No attribs, no subtags
        static ITag F2(ITag t) => t.T("t2", t => t["att1", "attval"].Empty()); //No attribs, no subtags

        Func<ITag, ITag> F3 = t => t.T("t3", (XTag t) => t.T("subtag")); //No attribs, Subtag
        Func<ITag, ITag> F4 = t => t.T("t4", t => t["att1", "attval"].T("subtag"));

        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        ITag resFTag = fTag.Ts("root", F1, F2, F3, F4);

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes); //Doesn't make sense!
    }


    [Fact]
    public void FourCombinationsAsLocalFunctions()
    {
        Func<XTag, ITag> AttribOnly = t => t["att1", "attval"].Empty();

        static ITag SubtagOnly(XTag t) => t.T("subtag"); //Local function
        Func<XTag, ITag> AttribAndSubtag = t => t["att1", "attval"].T("subtag");

        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        ITag resFTag = fTag.T("root", ts => ts
            .T("t1") //No attribs, no subtags
            .T("t2", AttribOnly) //No attribs, no subtags
            .T("t3", SubtagOnly) //No attribs, Subtag
            .T("t4", AttribAndSubtag)
            );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }
#pragma warning restore IDE0039 // Use local function

    [Fact]
    public void FourCombinations_IntoFile()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            using (StreamWriter wrtr = new(tempFile))
            {
                XTag fTag = new(wrtr);
                ITag resFTag = fTag.T("root", ts => ts
                    .T("t1") //No attribs, no subtags
                    .T("t2", t => t["att1", "attval"].Empty()) //No attribs, no subtags
                    .T("t3", t => t.T("subtag")) //No attribs, Subtag
                    .T("t4", t => t["att1", "attval"].T("subtag"))
                    );
            }
            var stringRes = File.ReadAllText(tempFile);
            Assert.Equal(fourTagsExample, stringRes);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }


    [Fact]
    public void FourCombinations()
    {
        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        ITag resFTag = fTag.T("root", ts => ts
            .T("t1") //No attribs, no subtags
            .T("t2", t => t["att1", "attval"].Empty()) //No attribs, no subtags
            .T("t3", t => t.T("subtag")) //No attribs, Subtag
            .T("t4", t => t["att1", "attval"].T("subtag"))
            );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }


    [Fact]
    public void EmptyTagWithinATag()
    {
        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        var resFTag = fTag.T("t1", t => t["at1", "val1"]
            .T("empty"));

        string expected = """
            <t1 at1="val1">
              <empty/>
            </t1>
            """.ReplaceLineEndings();

        var stringRes = wrtr.ToString();
        Assert.Equal(expected, stringRes);
    }


    [Fact]
    public void SingleEmptyTag()
    {
        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        ITag resFTag = fTag.T("empty");

        //Code demonstrating that the attributes following the tags can be detected at compile time
        //ITag _ = fTag.T("illegal", t => t["legalAtt", "val1"].T("legaltag")["illegalAtt", "val1"]); 

        var stringRes = wrtr.ToString();
        Assert.Equal("<empty/>", stringRes);
    }


    [Fact]
    public void TagWithTextWithinATagWithAttributes()
    {
        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        var _ = fTag.T("t1", t => t["at1", "val1"]
            .T("t2", text: "SomeText"));

        string expected = """
            <t1 at1="val1">
              <t2>SomeText</t2>
            </t1>
            """.ReplaceLineEndings();

        var stringRes = wrtr.ToString();
        Assert.Equal(expected, stringRes);
    }

    [Fact]
    public void ThreeLevels_DoubleAttributes()
    {
        using StringWriter wrtr = new();

        XTag fTag = new(wrtr);
        ITag _ = fTag.T("t1", t =>
            t["at1", "val1"].T("t2", t =>
                t["at21", "val21"]["at22", "val22"].T("t3", t =>
                    t.T("t4", text: "SomeText")
                )
            )
        );

        string expected = """
            <t1 at1="val1">
              <t2 at21="val21" at22="val22">
                <t3>
                  <t4>SomeText</t4>
                </t3>
              </t2>
            </t1>
            """.ReplaceLineEndings();

        var stringRes = wrtr.ToString();
        Assert.Equal(expected, stringRes);
    }
}
//.Tag("t2", text: "SomeText")