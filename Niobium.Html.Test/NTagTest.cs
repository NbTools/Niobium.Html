using System.Text;

namespace Niobium.Html.Test;

public class NTagTest
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
        static void F1(NTag t) => t.T("t1"); //No attribs, no subtags
        static void F2(NTag t) => t.T("t2", [("att1", "attval")]); //No attribs, no subtags

        Action<NTag> F3 = t => t.T("t3", t => t.T("subtag")); //No attribs, Subtag
        Action<NTag> F4 = t => t.T("t4", [("att1", "attval")], t => t.T("subtag"));

        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        fTag.Ts("root", F1, F2, F3, F4);

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes); //Doesn't make sense!
    }


    [Fact]
    public void FourCombinationsAsLocalFunctions()
    {
        var AttribOnly = ("att1", "attval");
        static void SubtagOnly(NTag t) => t.T("subtag");

        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        fTag.T("root", ts => ts
            .T("t1") //No attribs, no subtags
            .T("t2", [AttribOnly])  //Think about an ovrload for single tag 
            .T("t3", SubtagOnly)
            .T("t4", [AttribOnly], SubtagOnly)
            );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }
#pragma warning restore IDE0039 // Use local function

    [Fact]
    public void FourCombinations_IntoFile() //File is no longer used
    {
        StringBuilder wrtr = new();
        {
            NTag fTag = new(wrtr);
            fTag.T("root", ts => ts
                .T("t1") //No attribs, no subtags
                .T("t2", [("att1", "attval")]) //No attribs, no subtags
                .T("t3", t => t.T("subtag")) //No attribs, Subtag
                .T("t4", [("att1", "attval")], t => t.T("subtag"))
                );
        }
        Assert.Equal(fourTagsExample, wrtr.ToString());
    }


    [Fact]
    public void FourCombinations()
    {
        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        fTag.T("root", ts => ts
            .T("t1") //No attribs, no subtags
            .T("t2", [("att1", "attval")]) //No attribs, no subtags
            .T("t3", t => t.T("subtag")) //No attribs, Subtag
            .T("t4", [("att1", "attval")], t => t.T("subtag"))
            );

        var stringRes = wrtr.ToString();
        Assert.Equal(fourTagsExample, stringRes);
    }


    [Fact]
    public void EmptyTagWithinATag()
    {
        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        var resFTag = fTag.T("t1", [("at1", "val1")], t => t.T("empty"));

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
        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        fTag.T("empty");

        //Code demonstrating that the attributes following the tags can be detected at compile time
        //ITag _ = fTag.T("illegal", t => t["legalAtt", "val1"].T("legaltag")["illegalAtt", "val1"]); 

        var stringRes = wrtr.ToString();
        Assert.Equal("<empty/>", stringRes);
    }


    [Fact]
    public void TagWithTextWithinATagWithAttributes()
    {
        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        var _ = fTag.T("t1", [("at1", "val1")], t => t.T("t2", val: "SomeText"));

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
        StringBuilder wrtr = new();

        NTag fTag = new(wrtr);
        fTag.T("t1", [("at1", "val1")], t =>
            t.T("t2", [("at21", "val21"), ("at22", "val22")], t =>
                t.T("t3", t =>
                    t.T("t4", "SomeText")
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