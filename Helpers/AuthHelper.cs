namespace DotNetAPI.Helpers;

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotNetAPI.Data;
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

    private readonly DataContextDapper _dapper;

    public AuthHelper(IConfiguration config)
    {
        _config = config;
        _dapper = new DataContextDapper(config);
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

    public bool SetPassword(UserLoginDto userSettingPasword)
    {
        byte[] passwordSalt = new byte[128 / 8];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        // Make the password much bigger and much harder to brute force with all the random characters
        byte[] passwordHash = CreatePasswordHash(userSettingPasword.Password, passwordSalt);

        string addAuthQuery = $@"EXEC TutorialAppSchema.spUpsert_Registration
                @Email = @EmailParam,
                @Passwordhash = @PasswordHashParam,
                @PasswordSalt = @PasswordSaltParam";

        List<SqlParameter> sqlParameters = new List<SqlParameter>();

        SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar)
        {
            Value = userSettingPasword.Email
        };
        sqlParameters.Add(emailParameter);

        SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", SqlDbType.VarBinary)
        {
            Value = passwordSalt
        };
        sqlParameters.Add(passwordSaltParameter);

        SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary)
        {
            Value = passwordHash
        };
        sqlParameters.Add(passwordHashParameter);

        return _dapper.ExecuteSqlWithParameters(addAuthQuery, sqlParameters);
    }
}

