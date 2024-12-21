using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Niobium.Html.Test;

public class JsonConvertTest
{
    [Fact]
    public async Task HtmlInterceptor_Test()
    {
        string jsonText = File.ReadAllText($@"Data/1.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        JsonObject jObj = new(Intercept);
        string htmlResult = await CreateHtml($"Test1", t => jObj.Convert(obj, t));
        Assert.Contains("Overridden", htmlResult);
    }

    private static bool Intercept(Stack<string> propName, string? propValue, XTag tag)
    {
        if (propName.Match("glossary", "title"))
        {
            tag.Text("Overridden");
            return true;
        }
        return false;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FileBasedTest(int ind)
    {
        string jsonText = File.ReadAllText($@"Data/{ind}.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        JsonObject jObj = new();
        var htmlResult = await CreateHtml($"Test-{ind}", t => jObj.Convert(obj, t));
        htmlResult = htmlResult.ReplaceLineEndings();
        //File.WriteAllText($"{ind}.html", htmlResult); Uncomment to re-generate test files

        var htmlCheck = File.ReadAllText($@"Data/{ind}.html").ReplaceLineEndings();
        Assert.Equal(htmlCheck, htmlResult);
    }

    private static Task<string> CreateHtml(string header, Func<XTag, ITag> tag) => HtmlTag.HtmlPage2String(new HtmlParam(header), tag);


    /*[Fact]
    public void File2Part2_reTest()
    {
        string jsonText = """
         [
            {
                "value": "New",
            "onclick": "CreateNewDoc()"
            },
            {
                "value": "Open",
            "onclick": "OpenDoc()"
            },
            {
                "value": "Close",
            "onclick": "CloseDoc()"
            }
         ]
    """;
        JToken obj = JsonConvert.DeserializeObject(jsonText) as JToken ?? throw new Exception("Json doesn't contain JToken");

        JsonObject jObj = new();
        var htmlResult = JsonObject.CreateHtml($"Test1", t => jObj.Convert(obj, t));
        Assert.Contains("Overridden", htmlResult);
    }*/
}
