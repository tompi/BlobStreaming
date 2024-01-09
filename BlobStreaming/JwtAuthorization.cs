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
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey(Constants.VideoCookieName))
                    {
                        context.Token = context.Request.Cookies[Constants.VideoCookieName];
                    }
                    return Task.CompletedTask;
                }
            };
            var signingKey = keyVaultService.GetRsaSecurityKey().Result;
            Console.WriteLine(signingKey.KeyId);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidIssuer = Constants.Issuer,
                ValidAudience = Constants.Audience,
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