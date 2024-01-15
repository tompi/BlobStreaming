using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Storage.Blobs;
using BlobStreaming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);
var keyVaultService = new KeyVaultService("https://d-tcprlink-ne-kv-sg.vault.azure.net/keys/d-tcprlink-video-test/");

JwtAuthorization.SetupAuthorization(builder, keyVaultService);
var app = builder.Build();

app.UseDefaultFiles()
    .UseHsts()
    .UseStaticFiles()
    .UseAuthentication()
    .UseAuthorization();


app.MapGet("/set-cookie-and-goto-video", async (HttpContext httpContext) =>
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
    httpContext.Response.Redirect("/video.html");
    return "Redirecting to video.html";
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

app.MapGet("/stream/test-auth", [Authorize](HttpContext httpContext) =>
{
    var userName = httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
    return $"Authenticated as user: {userName}";
});

// This will return 401
app.MapGet("/differenturl/test-auth", [Authorize](HttpContext httpContext) =>
{
    var userName = httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
    return $"Authenticated as user: {userName}";
});

app.Run();

