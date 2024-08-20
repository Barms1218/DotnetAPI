namespace DotNetAPI.Helpers;

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Class contaning methods that
/// </summary>
public class AuthHelper
{
    private readonly IConfiguration _config;

    public AuthHelper(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a password hash using the provided user password and password salt.
    /// Combines a configured password key with the salt to derive the final hash using PBKDF2.
    /// </summary>
    /// <param name="password">The password that is going to get hashed.</param>
    /// <param name="passwordSalt">The byte array representing the password salt.</param>
    /// <returns>A byte array representing the generated password hash.</returns>
    public byte[] CreatePasswordHash(string password, byte[] passwordSalt)
    {
        string? passwordSaltString = _config.GetSection("AppSettings:PasswordKey").
        Value + Convert.ToBase64String(passwordSalt);

        return KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(passwordSaltString),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8);
    }

    /// <summary>
    /// Created a security key, credentials, and a descriptor to keep the user authenticated for one day
    /// </summary>
    /// <param name="userId">Value used to create a claim that the person is authenticated</param>
    /// <returns></returns>
    public string CreateToken(int userId)
    {
        int daysAuthenticated = 1;

        Claim[] claims = new Claim[]
        {
            new Claim("userId", userId.ToString())
        };

        // Token
        string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));

        // Signer
        SigningCredentials credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha512Signature);

        SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Expires = DateTime.Now.AddDays(daysAuthenticated)
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken token = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(token);
    }
}