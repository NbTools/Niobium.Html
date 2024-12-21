namespace Niobium.Html.Test.ExperimentVirt;

//Interface has only Tag functions
public interface ITTag
{
    ITTag TagBase(string name, Func<ITTag, ITTag> func); //Lowering
}

public class TTag : ITTag
{
    public TTag AttrBase(string name, Func<TTag, TTag> func) => this; //Not lowering

    public ITTag TagBase(string name, Func<ITTag, ITTag> func) => this; //Lower TTag to ITTag
}

public interface IDTag : ITTag
{
    IDTag TagDerived(string name, Func<IDTag, IDTag> func); //Lowering
}


public class DTag : IDTag
{
    public DTag AttrBase(string name, Func<DTag, DTag> func) => this;

    public IDTag TagBase(string name, Func<IDTag, IDTag> func) => this;

    public DTag AttrDerived(string name, Func<DTag, DTag> func) => this;

    public IDTag TagDerived(string name, Func<IDTag, IDTag> func) => this;

    public ITTag TagBase(string name, Func<ITTag, ITTag> func) => this;
}


public class ExperimentVirt
{

    [Fact]
    public void Tags_TEs()
    {
        //TTag tag = new();  //Doesnt' work 
        //var t1 = tag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).TagBase("t2", t => t);
        //var t2 = tag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).AttrBase("t2", t => t); //Attr doesn't work as expected

        DTag dtag = new();
        /*var d1 = dtag.AttrBase("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t).TagBase("t2", t => t); //Works as base class
        var d2 = dtag.AttrDerived("name1", t => t).AttrBase("name2", t => t).TagBase("t1", t => t); //Works as base class
        var d3 = dtag.AttrDerived("name1", t => t).TagBase("t1", t => t); //Works as base class
        var d4 = dtag.AttrBase("name1", t => t).AttrDerived("name2", t => t).TagBase("t1", t => t); //Works as base class*/
        var d4 = dtag.AttrBase("name1", t => t).AttrDerived("name2", t => t).TagBase("t1", t => t).TagDerived("t2", t => t); //Works as base class
        var d5 = dtag.AttrDerived("name1", t => t).AttrBase("name2", t => t).TagDerived("t1", t => t).TagBase("t2", t => t); //Works as base class

    }
}

