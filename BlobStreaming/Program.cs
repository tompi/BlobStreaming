using Azure.Storage.Blobs;
using BlobStreaming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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

app.MapGet("/set-cookie", async (HttpContext httpContext) =>
{
    var token = await keyVaultService.MakeJwt("12345", "stream");
    var options = new CookieOptions()
    {
        Path = "/stream",
        Expires = DateTimeOffset.Now.AddMinutes(Constants.TokenExpirationMinutes),
        Secure = true,
        IsEssential = true,
        SameSite = SameSiteMode.Strict,
    };
    httpContext.Response.Cookies.Append(Constants.VideoCookieName, token, options);
    return "Cookie set!";
});

var blobClient =new BlobContainerClient(
    "UseDevelopmentStorage=true",
    "vidtest");

app.MapGet("/stream",  [Authorize] [ResponseCache(NoStore = true)]
    async () =>
    {
        Console.WriteLine("Stream request");
        var client = blobClient.GetBlobClient("testfilm_large.mp4");
        var stream = await client.OpenReadAsync();
        return Results.Stream(stream, Constants.VideoContentType, enableRangeProcessing: true);
    });

app.Run();

