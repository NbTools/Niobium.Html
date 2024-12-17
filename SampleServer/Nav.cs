using System.Text;
using Niobium.Html;
using Niobium.Html.Async;

namespace SampleServer;

public static class NavReport
{
    public static async Task FileViewer(Stream stream, string filename)
    {
        string flname = Encoding.UTF8.GetString(Convert.FromBase64String(filename));
        await using StreamWriter wrtr = new(stream);

        ATag htmlTag = new(wrtr);
        await htmlTag.T("h3", flname)
            .T("pre", async t => await t.T("code", await File.ReadAllTextAsync(flname))
        ); 
    }


    public static ITag Run(HttpContext _, NHeader hdr, XTag t)
    {
        NavStyles(hdr);

        DirectoryInfo dir = new(Directory.GetCurrentDirectory());

        t.div(Sidenav, t =>
        {
            t["hx-boost", "true"]["hx-target", "#Content"].p("Menu");
            foreach (FileInfo di in dir.GetFiles())
                t.a($"/file/{Convert.ToBase64String(Encoding.UTF8.GetBytes(di.FullName))}", di.Name);
            return t;
        });

        t.div(Sidebody, a => a["id", "Content"]
            .h2("Content")
            .p(Directory.GetCurrentDirectory())
        );

        /*foreach ((string name, StringValues vals) in cont.Request.Headers)
        {
            t.h3(name);
            foreach (string? val in vals.Where(v => !String.IsNullOrEmpty(v)))
            {
                t.p(val!);
            }
        }*/

        return t;
    }

    private static void NavStyles(NHeader hdr)
    {
        //<script src="https://unpkg.com/htmx.org@2.0.3/dist/htmx.js" integrity="sha384-BBDmZzVt6vjz5YbQqZPtFZW82o8QotoM7RUp5xOxV3nSJ8u2pSdtzFAbGKzTlKtg" crossorigin="anonymous"></script>
        hdr.AddScriptUri(new Uri("https://unpkg.com/htmx.org@2.0.3/dist/htmx.js"));
        hdr.AddScriptUri(new Uri("https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js"));
        hdr.AddScript("""
            $(document).ready(function() {
              $('.sidenav a').click(function() {
                $('.sidenav a').removeClass('active');
                $(this).addClass('active');
                });
            });
            """);
    }

    const string sidebarWidth = "180px";

    private static readonly NCssAttrib SidenavA = new(".sidenav a", [
        ("padding", "2px 10px"),
        ("text-decoration", "none"),
        ("font-size", "13px"),
        ("display", "block"),
        ("line-height", " 1.5"),
    ]);
    //hdr.TryAdd(SidenavA);

    private static readonly NCssAttrib SidenavAActive = new(".sidenav a.active", [
        ("background-color", "#04AA6D !important"),
        ("color", "#ffffff !important"),
    ]);
    //hdr.TryAdd(SidenavAActive);

    private static readonly NCssAttrib Sidenav = new(".sidenav", [
        ("height", "100%"),
        ("width", sidebarWidth),
        ("position", "fixed"),
        ("z-index", "1"),
        ("top", "0"),
        ("left", "0"),
        ("overflow-x", "hidden"),
        ],
        children: [SidenavA, SidenavAActive]
    );



    private static readonly NCssAttrib Sidebody = new(".sidebody", [
        ("margin-left", sidebarWidth),
        ("padding", "0px 10px"),
    ]);

}
