namespace BlobStreaming;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

public static class JwtAuthorization
{
    public static void SetupAuthorization(WebApplicationBuilder builder, KeyVaultService keyVaultService)
    {
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = keyVaultService.GetRsaSecurityKey().Result,
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
    }
}