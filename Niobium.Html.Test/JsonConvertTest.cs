using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Niobium.Html.Test;

public class JsonConvertTest
{
    [Fact]
    public void HtmlInterceptor_Test()
    {
        string jsonText = File.ReadAllText($@"Data/1.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        JsonObject jObj = new(Intercept);
        var htmlResult = JsonObject.CreateHtml($"Test1", t => jObj.Convert(obj, t));
        Assert.Contains("Overridden", htmlResult);
    }

    private static bool Intercept(Stack<string> propName, string? propValue, Tag tag)
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
    public void FileBasedTest(int ind)
    {
        string jsonText = File.ReadAllText($@"Data/{ind}.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        JsonObject jObj = new();
        var htmlResult = JsonObject.CreateHtml($"Test-{ind}", t => jObj.Convert(obj, t));
        htmlResult = htmlResult.Replace("\r\n", "\n");
        //File.WriteAllText($"{ind}.html", htmlResult); Uncomment to re-generate test files

        var htmlCheck = File.ReadAllText($@"Data/{ind}.html").Replace("\r\n", "\n");
        Assert.Equal(htmlCheck, htmlResult);
    }
}
