using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/stream", () =>
{
    var video = File.OpenRead("testfilm.mp4");

    return Results.Stream(video, "video/mp4", enableRangeProcessing: true);
});

app.Run();