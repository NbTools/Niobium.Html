using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Niobium.Html.Test;

public class NbJsonMatrix
{
    [Fact]
    public void Test1()
    {
        string jsonText = File.ReadAllText(@"Data/1.json");
        JObject obj = JsonConvert.DeserializeObject(jsonText) as JObject ?? throw new Exception("Json doesn't contain JObject");

        StringBuilder bld = new();
        var nbTag = NbTag.Create(bld);
        JsonToHtml.Convert(obj, nbTag);

        var html = JsonToHtml.CreateHtml("header", t => JsonToHtml.Convert(obj, t));
        html = html.Replace("\r\n", "\n");
        Assert.Equal(File.ReadAllText(@"Data/1.html"), html);
    }
}
