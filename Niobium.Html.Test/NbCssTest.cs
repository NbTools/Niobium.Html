﻿namespace Niobium.Html.Test;

public class NbCssTest
{
    [Fact]
    public void HtmlTag_Test()
    {
        string img = @"url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAA7EAAAOxAGVKw4bAAABcElEQVR4Xq2TsUsCURzHv15g8ZJcBWlyiYYgCIWcb9DFRRwMW5TA2c0/QEFwFkxxUQdxVlBwCYWOi6IhWgQhBLHJUCkhLr/BW8S7gvrAg+N+v8/v+x68Z8MGy+XSCyABQAXgBgHGALoASkIIDWSLeLBetdHryMjd5IxQPWT4rn1c/P7+xxp72Cs9m5SZ0Bq2vPnbPFafK2zDvmNHypdC0BPkLlQhxJsCAhQoZwdZU5mwxh720qGo8MzTxTTKZDPCx2HoVzp6lz0Q9tKhyx0kGs8Ny+TkWRKk8lCROwEduhyg9l/6lunOPSfmH3NUH6uQ0KHLAe7JYvJjevm+DAMGJHToKtigE+vwvIidxLamb8IBY9e+C5LiXREkfho3TSd06HJA13/oh6T51MTsfQbHrsMynQ5dDihFjiK8JJAU9AKIWTp76dCVN7HWHrajmUEGvyF9nkbAE6gLIS7kTUyuf2gscLoJrElZo/Mvj+nPz/kLTmfnEwP3tB0AAAAASUVORK5CYII=)";

        NCssAttrib IconSuccessEncoded = new NCssAttrib(".IconSuccessEncoded")["background-image", img]
          ["min-width", "18px"]["min-height", "18px"]["background-repeat", "no-repeat"]["background-position", "center"];

        HtmlParam par = new("Title");
        var res = HtmlTag.CreateHtmlPage(par, t => t.T("h1", "Header").T("p", "parapraph")
            .T("p", t => t.T("img", "IconSuccessEncoded class must go here"))   //TODO: deal with Head  //IconSuccessEncoded
            .T("img", "IconSuccessEncoded must go here") //Added twice, the class should appear only once
        );
        Assert.NotEmpty(res);
        Assert.Contains(img, res);
        //File.WriteAllText(@"C:\AutoDelete\test.html", res);
        //Process.Start(@"C:\Program Files\Mozilla Firefox\firefox.exe", @"C:\AutoDelete\test.html");
    }

    [Fact]
    public void NbCss_Test()
    {
        NHeader nbCss = new();
        string res = nbCss.ToString();
        Assert.NotEmpty(res);
    }
}
