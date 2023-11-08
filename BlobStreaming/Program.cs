using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
        options =>
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(
                @"
-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA2kgvT1Gyrcu7cTMFSzcq
5G+tQ+1afQ6v8FkgemAMxiq6/OiCkNFfQ3zX44sV0xu0MUw2XrcO7qbuNnTVZqEF
t8TOLH3tYMDGrppL7d624/6KND2qnSJgd8bVe7dYxYgzLh4+s2eABkSuuMlCV+d+
gaEys1lRAZ1gBcIAVen2ofz/t7utuZ7Q3LGPlglhLAxYOmxfs2/43c3V3aYIbFnH
NEOqoHpah8Ms75uMriw8WbcP4di7FvLX1/7Pu/sXRYZkLW58HtB7roe7ts8QePYm
CkxTKq/BdzjxBeubMJxeeP0yE6F6p6xdz278gBVQ5XVrmqvstY9AORZITqgv7bog
6YdKezqCYh/CL9H7JGxJ8DiYvG2ImSkzuFXaF/iqgMGK8XQE0y7JTYzpPPNawktC
haVziLOm3jOZV/2vGKCVc4rMnerC4MRP9jc2gcfpzcUh/N/R93gtOMHFtAWyIBiQ
n6bfTw8sCJBjz7FPcR8Lk1cscRgumQmzalLOrMclbzq+vDml/jcLPEEgBIfCZXs6
//Hv+CNWzr6nDCet1zRm7JI+6oxnSflD9QJqw9lbbBkoIa8t0vRyL4MLTUBdGAJE
6bRfRYn8Sc4MYS0gA7izpsSjdydQ1u7f7dP5rmOVruI1F16UYjLzAmXz4ABsjwii
PKzjaKmh4K3XoK7zFCw6cL0CAwEAAQ==
-----END PUBLIC KEY-----"
                    .ToCharArray());
            var publicKey = new RsaSecurityKey(rsa);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = publicKey,
                ValidIssuer = "https://www.tcpr.link/",
                ValidAudience = "the_world",
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(10),
            };
        });

builder.Services.AddAuthorization(
    options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme);
            defaultAuthorizationPolicyBuilder =
                defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        });

var app = builder.Build();

app.UseDefaultFiles()
    .UseHsts()
    .UseStaticFiles()
    .UseAuthentication()
    .UseAuthorization();

// Valid token: eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJteVJlc3BvbmRlciIsImp0aSI6IjkxMmRjOTkwLThhZDItNDdkNS1iOWIyLWI4YjQwZGMyZGM0MSIsIm5iZiI6MTY0NjMxMzQ0MiwiZXhwIjoxODA0MDc5ODQyLCJpc3MiOiJodHRwczovL3d3dy50Y3ByLmxpbmsvIiwiYXVkIjoidGhlX3dvcmxkIn0.sDWZ3mNiQ0nkiTgddbgSClxYtUCA_ddIT7_gM8R2DjlUn7eswugYnPnn-38ds4LkoE4HACcwIcfk29KFOo_ewLWA-MREgqIP2CTg1g5uaB8CJj94eikG2fLBPpv-5nuEmHg3e0ovQlB7KwMSU-GWMtWZBSwae4cgW0I1Kxi9rqeot2zki6QXwW_XSe4o4yzyKw87vKZrKz-N60xU9Th22TBgZHSXn8GWfK6B4c3sG3s-CHzbEA0smyemFjd787MYId129UfVwi0OFXm-bW6YieLAKonmp9bKEmLhN_JM5eVHlbGS6q7X0n-9BFzeEcHffW1Pi-ZUWIOROivnS5SlOhkTEETYN6QAkEWjetRQbxpOIGnLBXmWywJmZIrhCkr5XSUdCuDBE0t7prgVb3r75bwwGfYMvANyJPAG4rS7q5s2egAIqC3u8kafL7XrjJ8uJgl_2wjuQ2hyFTskXmRgV_K5Y1KKo0sA57IzT6ggJcIUvfvPxLggAz_4eimr51Kj6XYj0YLbGCtZ7oKfaXmqR0Lw-WtsajTYUoE_uYf5tF9LMMXwvpf4UDg-K9Nmpwxfna5pvbf7VKXPe4Psr_wg9oZFn9GhfgnSPrzm38oBOCkstRXikmL_idvv8RBkYQeloiPRthVnl4SeaFy78gMZGI073XTZFTJBwFXKLnCLASI
app.MapGet("/test-auth", [Authorize](HttpContext httpContext) =>
{
    var c = httpContext.User;
    return "Authenticated!";
});

app.MapGet("/stream", () =>
{
    var video = File.OpenRead("testfilm.mp4");
    return Results.Stream(video, "video/mp4", enableRangeProcessing: true);
});

app.Run();