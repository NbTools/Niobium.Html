using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Niobium.Html.Test;

public class NbJsonMatrix
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Test1(int ind)
    {
        string jsonText = File.ReadAllText($@"Data/{ind}.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        StringBuilder bld = new();
        var nbTag = Tag.Create(bld);
        JsonToHtml.Convert(obj, nbTag);

        var htmlResult = JsonToHtml.CreateHtml($"Test-{ind}", t => JsonToHtml.Convert(obj, t));
        htmlResult = htmlResult.Replace("\r\n", "\n");
        //File.WriteAllText($"{ind}.html", htmlResult); Uncomment to re-generate test files

        var htmlCheck = File.ReadAllText($@"Data/{ind}.html").Replace("\r\n", "\n");
        Assert.Equal(htmlCheck, htmlResult);
    }
}
