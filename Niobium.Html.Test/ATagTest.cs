using Niobium.Html.Async;

namespace Niobium.Html.Test;

public class ATagTest
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
    public async void FourCombinationsAsParams_Tags()
    {
        //Local functions
        static Task<IATag> F1(IATag t) => t.T("t1"); //No attribs, no subtags
        static Task<IATag> F2(IATag t) => t.T("t2", t => t.A("att1", "attval").Empty()); //No attribs, no subtags

        Func<IATag, Task<IATag>> F3 = t => t.T("t3", (ATag t) => t.T("subtag")); //No attribs, Subtag
        Func<IATag, Task<IATag>> F4 = t => t.T("t4", t => t.A("att1", "attval").T("subtag"));

        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag resFTag = await fTag.Ts("root", F1, F2, F3, F4);

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes); //Doesn't make sense!
    }


    [Fact]
    public async void FourCombinationsAsLocalFunctions()
    {
        Func<ATag, Task<IATag>> AttribOnly = t => t.A("att1", "attval").Empty();

        static Task<IATag> SubtagOnly(ATag t) => t.T("subtag"); //Local function
        Func<ATag, Task<IATag>> AttribAndSubtag = t => t.A("att1", "attval").T("subtag");

        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag resFTag = await fTag.T
        ("root", async ts => await
            (
                await (
                    await (
                        await ts.T("t1")
                    ).T("t2", AttribOnly)
                ).T("t3", SubtagOnly)
            ).T("t4", AttribAndSubtag)
        );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }
#pragma warning restore IDE0039 // Use local function

    [Fact]
    public async void FourCombinations_IntoFile()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            using (StreamWriter wrtr = new(tempFile))
            {
                ATag fTag = new(wrtr);
                IATag resFTag = await fTag.T("root", async ts =>
                {
                    await ts.T("t1"); //No attribs, no subtags
                    await Task.Delay(300);
                    await ts.T("t2", t => t.A("att1", "attval").Empty()); //No attribs, no subtags
                    await ts.T("t3", async t =>
                    {
                        await Task.Delay(200);
                        await t.T("subtag");
                        return t;
                    }
                    ); //No attribs, Subtag
                    await ts.T("t4", t => t.A("att1", "attval").T("subtag"));
                    return ts;
                });
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
    public async Task FourCombinations()
    {
        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag resFTag = await fTag.T("root", ts => ts
            .T("t1") //No attribs, no subtags
            .T("t2", t => t.A("att1", "attval").Empty()) //No attribs, no subtags
            .T("t3", t => t.T("subtag")) //No attribs, Subtag
            .T("t4", t => t.A("att1", "attval").T("subtag"))
            );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }


    [Fact]
    public async Task EmptyTagWithinATag()
    {
        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag resFTag = await fTag.T("t1", t => t.A("at1", "val1")
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
    public async Task SingleEmptyTag()
    {
        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag resFTag = await fTag.T("empty");

        //Code demonstrating that the attributes following the tags can be detected at compile time
        //IATag _ = fTag.T("illegal", t => t["legalAtt", "val1"].T("legaltag")["illegalAtt", "val1"]); 

        var stringRes = wrtr.ToString();
        Assert.Equal("<empty/>", stringRes);
    }


    [Fact]
    public async Task TagWithTextWithinATagWithAttributes()
    {
        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag _ = await fTag.T("t1", async t => await t.A("at1", "val1")
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
    public async Task ThreeLevels_DoubleAttributes()
    {
        using StringWriter wrtr = new();

        ATag fTag = new(wrtr);
        IATag _ = await fTag.T("t1", t =>
            t.A("at1", "val1").T("t2", t =>
                t.A("at21", "val21").A("at22", "val22").T("t3", t =>
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