using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace BlobStreaming;

/// <summary>
/// Copied from https://stackoverflow.com/questions/69117288/sign-jwt-token-using-azure-key-vault
/// </summary>
public class KeyVaultService
{
    private readonly string _keyVaultUrl;
    private SHA256? _sha256;
    private CryptographyClient? _cryptoClient;
    private string _publicRsaKey;
    private RsaSecurityKey _rsaSecurityKey;

    public KeyVaultService(string keyVaultUrl)
    {
        _keyVaultUrl = keyVaultUrl;
    }

    public async Task<RsaSecurityKey> GetRsaSecurityKey()
    {
        await EnsureInitialized();

        return _rsaSecurityKey;
    }

    public async Task Sign(string userId, string jti)
    {
        await EnsureInitialized();

        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
        };

        var header = @"{""alg"":""RS256"",""typ"":""JWT""}";
        var payload = new JwtPayload(
            "https://liveborn.laerdal.com/",
            "https://liveborn.laerdal.com/",
            claims,
            now.AddMilliseconds(-30),
            now.AddMinutes(120));
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerAndPayload = $"{Base64UrlEncoder.Encode(header)}.{Base64UrlEncoder.Encode(payloadJson)}";

        var digest = _sha256.ComputeHash(Encoding.ASCII.GetBytes(headerAndPayload));
        var signature = (await _cryptoClient.SignAsync(SignatureAlgorithm.RS256, digest)).Signature;

        var token = $"{headerAndPayload}.{Base64UrlEncoder.Encode(signature)}";
        Console.WriteLine($"JWT:\n\n{token}");
    }

    private async Task EnsureInitialized()
    {
        if (_cryptoClient != null)
        {
            return;
        }

        var uri = new Uri(_keyVaultUrl, UriKind.Absolute);
        var id = new KeyVaultKeyIdentifier(uri);
        var credential = new DefaultAzureCredential();

        var keyClient = new KeyClient(id.VaultUri, credential);
        KeyVaultKey key = await keyClient.GetKeyAsync(id.Name, id.Version);

        using var rsaKey = key.Key.ToRSA();
        _rsaSecurityKey = new RsaSecurityKey(rsaKey);

        _sha256 = SHA256.Create();
        _cryptoClient = new CryptographyClient(key.Id, credential);
    }
}