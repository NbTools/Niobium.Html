using System.Text;

namespace Niobium.Html.Test;

public class CsvToolTest
{
    [Fact]
    public void BasicTest()
    {
        StringBuilder bld = new();
        string[] line = CsvTool.DeCsvLine("""1,,"","One,Two,Three",Four,"5",6""", bld).ToArray();
        string[] exptected = ["1", "", "", "One,Two,Three", "Four", "5", "6"];
        Assert.True(exptected.SequenceEqual(line));
    }
}
