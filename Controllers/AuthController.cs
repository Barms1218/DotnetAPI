using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public AuthController(IConfiguration config)
    {
        _config = config;
        _dapper = new DataContextDapper(config);
    }


    #region Endpoints

    /// <summary>
    /// Handles user registration by checking for existing users, generating a password hash and salt,
    /// and inserting the user credentials into the database.
    /// </summary>
    /// <param name="user">The user data transfer object containing registration details.</param>
    /// <returns>An IActionResult indicating the outcome of the registration process.</returns>
    /// <exception cref="Exception">Thrown if the passwords do not match, the user already exists, 
    /// or if there is an error during the registration process.</exception>
    [AllowAnonymous] // Allowed to receive an anonymous request, does not require a token
    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto user)
    {
        if (user.Password == user.PassWordConfirm)
        {
            string query = $@"SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '{user.Email}'";

            IEnumerable<string> existingUsers = _dapper.GetRows<string>(query);
            if (existingUsers.Count() == 0)
            {
                byte[] passwordSalt = new byte[128 / 8];

                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                // Make the password much bigger and much harder to brute force with all the random characters
                byte[] passwordHash = CreatePasswordHash(user.Password, passwordSalt);

                string addAuthQuery = $@"INSERT INTO TutorialAppSchema.Auth ([Email],
                [Passwordhash],
                [PasswordSalt]) VALUES (
                '{user.Email}', @PasswordHash, @PasswordSalt)";

                List<SqlParameter> sqlParameters = CreateSqlParameters(passwordSalt, passwordHash);

                if (_dapper.ExecuteSqlWithParameters(addAuthQuery, sqlParameters))
                {
                    string createUserQuery = CreateUser(user);

                    if (_dapper.ExecuteSql(createUserQuery))
                    {
                        return Ok();
                    }
                    throw new Exception("Failed to add user.");
                }

                throw new Exception("Failed to register user.");

            }
            throw new Exception("User with this email already exists.");
        }

        throw new Exception("Passwords do not match.");
    }


    [AllowAnonymous] // Allowed to receive an anonymous request, does not require a token
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto user)
    {
        string sqlForHashAndSalt = $@"SELECT [PasswordHash], 
        [PasswordSalt] 
        FROM TutorialAppSchema.Auth WHERE Email = '{user.Email}'";

        UserForLoginConfirmationDto userConfirmation = _dapper.
        GetSingleRow<UserForLoginConfirmationDto>(sqlForHashAndSalt);


        byte[] passwordHash = CreatePasswordHash(user.Password, userConfirmation.PasswordSalt);

        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != userConfirmation.PasswordHash[i])
            {
                return StatusCode(401, "Incorrect Password.");
            }
        }

        string userIdQuery = $@"SELECT UserId FROM TutorialAppSchema.Users 
        WHERE Email = '{user.Email}'";

        int userId = _dapper.GetSingleRow<int>(userIdQuery);

        return Ok(new Dictionary<string, string>()
        {
            {"token", CreateToken(userId)}
        });
    }

    /// <summary>
    /// Grabs the first user id from the Claims 
    /// Sql query to get the real numerical value and create a new token using that value
    /// </summary>
    /// <returns>A dictionary of one element containing the key of token with the user's ID as the value</returns>
    [HttpGet("RefreshToken")]
    public IActionResult RefreshToken()
    {
        string? userIdString = User.FindFirst("userId")?.Value;

        string userIdQuery = $@"SELECT UserId FROM TutorialAppSchema.Users
        WHERE UserId = '{userIdString}'";

        int userIdNum = _dapper.GetSingleRow<int>(userIdQuery);

        return Ok(new Dictionary<string, string> {
            {"token", CreateToken(userIdNum)}
        });
    }


    #endregion


    #region  Private Methods

    /// <summary>
    /// Creates a list of SQL parameters for a password salt and password hash.
    /// </summary>
    /// <param name="passwordSalt">The byte array representing the password salt.</param>
    /// <param name="passwordHash">The byte array representing the password hash.</param>
    /// <returns>A list of SqlParameter objects containing the password salt and hash.</returns>

    private List<SqlParameter> CreateSqlParameters(byte[] passwordSalt, byte[] passwordHash)
    {
        List<SqlParameter> sqlParameters = new List<SqlParameter>();

        SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
        passwordSaltParameter.Value = passwordSalt;

        SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
        passwordHashParameter.Value = passwordHash;

        sqlParameters.Add(passwordSaltParameter);
        sqlParameters.Add(passwordHashParameter);
        return sqlParameters;
    }

    /// <summary>
    /// Generates a password hash using the provided user password and password salt.
    /// Combines a configured password key with the salt to derive the final hash using PBKDF2.
    /// </summary>
    /// <param name="password">The password that is going to get hashed.</param>
    /// <param name="passwordSalt">The byte array representing the password salt.</param>
    /// <returns>A byte array representing the generated password hash.</returns>
    private byte[] CreatePasswordHash(string password, byte[] passwordSalt)
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
    private string CreateToken(int userId)
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

    /// <summary>
    /// Insert a new user into the Users database table
    /// </summary>
    /// <param name="user">The user which will be inserted into the table.</param>
    private static string CreateUser(UserForRegistrationDto user)
    {
        return $@"
                    INSERT INTO TutorialAppSchema.Users(
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                    ) VALUES (
                        '{user.FirstName}',
                        '{user.LastName}',
                        '{user.Email}',
                        '{user.Gender}',
                        1
                    )";
    }

    #endregion
}