using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gateway.Tests.Integration.Helpers;

/// <summary>
/// Helper class para generar tokens JWT para testing
/// </summary>
public static class JwtTokenGenerator
{
    private static readonly RSA _rsa = RSA.Create(2048);
    private static readonly RsaSecurityKey _securityKey = new RsaSecurityKey(_rsa);
    private static readonly SigningCredentials _signingCredentials = 
        new SigningCredentials(_securityKey, SecurityAlgorithms.RsaSha256);

    /// <summary>
    /// Genera un token JWT válido con los claims especificados
    /// </summary>
    public static string GenerateToken(
        string username,
        string email,
        string[] roles,
        string issuer = "http://localhost:8180/realms/Kairo",
        string audience = "kairo-api",
        int expirationMinutes = 30)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim("preferred_username", username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Genera un token JWT expirado
    /// </summary>
    public static string GenerateExpiredToken(
        string username,
        string email,
        string[] roles,
        string issuer = "http://localhost:8180/realms/Kairo",
        string audience = "kairo-api")
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim("preferred_username", username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(-1), // Expirado hace 1 hora
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Genera un token JWT con firma inválida
    /// </summary>
    public static string GenerateInvalidSignatureToken(
        string username,
        string email,
        string[] roles,
        string issuer = "http://localhost:8180/realms/Kairo",
        string audience = "kairo-api")
    {
        // Usar una clave diferente para la firma
        using var differentRsa = RSA.Create(2048);
        var differentKey = new RsaSecurityKey(differentRsa);
        var differentCredentials = new SigningCredentials(differentKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim("preferred_username", username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = differentCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Obtiene la clave pública RSA para validación
    /// </summary>
    public static RsaSecurityKey GetPublicKey()
    {
        return _securityKey;
    }
}
