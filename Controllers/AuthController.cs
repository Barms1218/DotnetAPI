namespace DotNetAPI.Controllers;

using System.Data;
using System.Security.Cryptography;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration config)
    {
        _config = config;
        _dapper = new DataContextDapper(config);
        _authHelper = new AuthHelper(config);
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
    public IActionResult Register(UserRegistrationDto user)
    {
        if (user.Password == user.PassWordConfirm)
        {
            string query = $@"EXEC TutorialAppSchema.spVerify_User @Email = '{user.Email}'";

            Console.Write(query);

            IEnumerable<string> existingUsers = _dapper.GetRows<string>(query);
            if (existingUsers.Count() == 0)
            {
                byte[] passwordSalt = new byte[128 / 8];

                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                // Make the password much bigger and much harder to brute force with all the random characters
                byte[] passwordHash = _authHelper.CreatePasswordHash(user.Password, passwordSalt);

                string addAuthQuery = $@"EXEC TutorialAppSchema.spUpsert_Registration
                @Email = @EmailParam,
                @Passwordhash = @PasswordHashParam,
                @PasswordSalt = @PasswordSaltParam";

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar)
                {
                    Value = user.Email
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

                if (_dapper.ExecuteSqlWithParameters(addAuthQuery, sqlParameters))
                {
                    string createUserQuery = $@" EXEC TutorialAppSchema.spUpsert_User
                                                @Firstname = '{user.FirstName}',
                                                @LastName = '{user.LastName}',
                                                @Email = '{user.Email}',
                                                @Gender = '{user.Gender}',
                                                @Active = {1},
                                                @Department = '{user.Department}',
                                                @JobTitle = '{user.JobTitle}',
                                                @Salary = '{user.Salary}'";

                    Console.WriteLine(createUserQuery);

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
    public IActionResult Login(UserLoginDto user)
    {
        string sqlForHashAndSalt = $@"SELECT [PasswordHash], 
        [PasswordSalt] 
        FROM TutorialAppSchema.Auth WHERE Email = '{user.Email}'";

        UserLoginConfirmationDto userConfirmation = _dapper.
        GetSingleRow<UserLoginConfirmationDto>(sqlForHashAndSalt);


        byte[] passwordHash = _authHelper.CreatePasswordHash(user.Password, userConfirmation.PasswordSalt);

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
            {"token", _authHelper.CreateToken(userId)}
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
            {"token", _authHelper.CreateToken(userIdNum)}
        });
    }


    #endregion

}