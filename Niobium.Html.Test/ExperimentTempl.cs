namespace Niobium.Html.Test.Temple;

public delegate Tag TagAction<in Attr, out Tag>(Attr input)
    where Attr : Tag
    where Tag : ITag;

public class ExperimentTempl
{
}

//Experimen with templates

//Tag has full set: attr AND tag
public class TTag<D> : ITTag<D> where D : TTag<D>
{
    public TTag<D> AttrBase(string name, Func<TTag<D>, TTag<D>> func) => this; //Not lowering

    public ITTag<D> TagBase(string name, Func<TTag<D>, ITTag<D>> func) => this; //Lower TTag to ITTag
}

//Interface has only Tag functions
public interface ITTag<D> where D : TTag<D>
{
    ITTag<D> TagBase(string name, Func<TTag<D>, ITTag<D>> func); //Lowering

}


public class DTag : TTag<DTag>
{
    public TTag<DTag> AttrDerived(string name, Func<TTag<DTag>, TTag<DTag>> func) => this;

}


public class NewTagsTest
{

    [Fact]
    public void Tags_TEs()
    {
        DTag dtag = new();
        var d1 = dtag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).TagBase("t2", t => t); //Works as base class
        var d2 = dtag.AttrDerived("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t); //Works as base class
        var d3 = dtag.AttrDerived("name1", t => t).TagBase("t1", t => t); //Works as base class
        //var d4 = dtag.AttrBase("name1", t => t).AttrDerived("name2", t => t).TagBase("t1", t => t); //Works as base class

        //TTag<TTag> tag = new();  //Doesnt' work 
        //var t1 = tag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).TagBase("t2", t => t);
        //var t2 = tag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).AttrBase("t2", t => t); //Attr doesn't work as expected
    }
}
