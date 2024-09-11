using HtmlAgilityPack;

namespace Niobium.Html.Test;

public class HelpersTest
{
    [Fact]
    public void StackMatching_Test()
    {
        Stack<string> stack = new(["GrandParent", "Parent", "Child"]);
        Assert.False(stack.Match("Bla", "GrandParent", "Parent", "Child"));


        Assert.True(stack.Match("Child"));
        Assert.True(stack.Match("Parent", "Child"));
        Assert.True(stack.Match("GrandParent", "Parent", "Child"));

        Assert.False(stack.Match(""));
        Assert.False(stack.Match("Parent"));
        Assert.False(stack.Match("GrandParent"));
        Assert.False(stack.Match("GrandParent", "Parent"));


        Assert.False(stack.Match("Bla"));
        Assert.False(stack.Match("Bla", "Child"));
        Assert.False(stack.Match("Bla", "Parent", "Child"));
    }
}

internal record HtmlRow(List<HtmlNode> Cells)
{
    internal HtmlRow(HtmlNodeCollection nodes) : this(nodes.Where(t => t is not HtmlTextNode).ToList()) { }
    internal string this[int i]
    {
        get => Cells[i].InnerText.Trim();
    }
}

internal class HtmlHelper
{
    /// <summary>
    /// Parses html containing table and return the collection of HtmlRow with Cells inside
    /// </summary>
    internal static List<HtmlRow> ParseTable(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        HtmlNode a = doc.DocumentNode.SelectNodes("//table").Single();

        var rows = a.ChildNodes.Where(t => t is not HtmlTextNode).Select(t => new HtmlRow(t.ChildNodes)).ToList();
        return rows;
    }
}
