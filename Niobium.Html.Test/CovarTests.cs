namespace Niobium.Html.Test;

public delegate Tag TagAction<in Attr, out Tag>(Attr input)
    where Attr : Tag
    where Tag : ITag;

public class CovarTests
{
}