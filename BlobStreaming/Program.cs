using BlobStreaming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var keyVaultService = new KeyVaultService("https://d-tcprlink-ne-kv-sg.vault.azure.net/keys/d-tcprlink-video-test/f6d4bded27674d3b9ad16618ed746b23");

JwtAuthorization.SetupAuthorization(builder, keyVaultService);
var app = builder.Build();



app.UseDefaultFiles()
    .UseHsts()
    .UseStaticFiles()
    .UseAuthentication()
    .UseAuthorization();



app.MapGet("/test-auth", [Authorize](HttpContext httpContext) =>
{
    var c = httpContext.User;
    return "Authenticated!";
});

app.MapGet("/set-cookie", [Authorize](HttpContext httpContext) =>
{
    var c = httpContext.User;
    var cookie = new CookieHeaderValue("video-user");
    cookie.Value = c.ToString();

    return "Authenticated!";
});


app.MapGet("/stream", () =>
{
    Console.WriteLine("Stream request");
    var video = File.OpenRead("testfilm_large.mp4");
    return Results.Stream(video, "video/mp4", enableRangeProcessing: true);
});

app.Run();

