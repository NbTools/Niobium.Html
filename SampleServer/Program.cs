using System.Text;
using Niobium.Html;
using SampleServer;

const string contTypeHtml = "text/html; charset=UTF-8";
IResult HtmlContent(string str) => Results.Content(str, contTypeHtml);

var bld = WebApplication.CreateBuilder(args);
// Add services to the container.
bld.Services.AddAuthorization();

//TODO: remove when writing is asynchronous
//using Microsoft.AspNetCore.Server.Kestrel.Core;
//bld.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });// If using Kestrel:
//bld.Services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });     // If using IIS:

var app = bld.Build();

app.UseHttpsRedirection(); // Configure the HTTP request pipeline.
app.UseAuthorization();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", WeatherForecast);

app.MapGet('/' + nameof(Test1), Test1);
app.MapGet('/' + nameof(TestStream), () => Results.Stream(st => TestStream(st), contTypeHtml));
app.MapGet('/' + nameof(Nav), (HttpContext cont) => Results.Stream(st => Nav(st, cont), contTypeHtml));
app.MapGet("/file1/{filename}", (HttpContext cont, string filename) =>
    Results.File(Encoding.UTF8.GetString(Convert.FromBase64String(filename)), "text/plain"));

app.MapGet("/file/{filename}", (HttpContext cont, string filename) => Results.Stream(st => NavReport.FileViewer(st, filename), contTypeHtml));

app.MapGet("/streamtxt", () => Results.Stream(st => StreamingResp(st), "text/plain"));  //Example of a streaming Minimal API

app.Run();

async Task StreamingResp(Stream str)
{
    using StreamWriter wrtr = new(str);

    for (int i = 1; i <= 100; i++)
    {
        await wrtr.WriteLineAsync($"Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} Line #{i} ");
        //await wrtr.FlushAsync();
        await Task.Delay(100);
    }
    await wrtr.WriteLineAsync("Done!");
}

async Task TestStream(Stream stream)
{
    await using StreamWriter wrtr = new(stream);
    await HtmlTag.StreamHtmlPage(wrtr, new HtmlParam("Title"), t => t.h1("Some text"));
}

async Task Nav(Stream stream, HttpContext cont)
{
    NHeader hdr = new();
    await using StreamWriter wrtr = new(stream);
    await HtmlTag.StreamHtmlPage(wrtr, new HtmlParam("Navigation", hdr), t => NavReport.Run(cont, hdr, t));
}

IResult Test1(HttpContext cont)
{
    string str = HtmlTag.HtmlPage2String(new HtmlParam("Title"), t => t.h1("Some text")).Result;
    return HtmlContent(str);
}















Weather[] WeatherForecast(HttpContext httpContext) =>
    Enumerable.Range(1, 5).Select(index =>
        new Weather(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)), //DateOnly.FromDateTime
            TemperatureC: Random.Shared.Next(-20, 55),
            Summary: summaries[Random.Shared.Next(summaries.Length)]
        )
    ).ToArray();

record Weather(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


