namespace DotNetAPI.Controllers;

using System.Data;
using System.Security.Cryptography;
using Dapper;
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
            string query = $@"EXEC TutorialAppSchema.spVerify_User 
            @Email = @EmailParam";

            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@EmailParam", user.Email, DbType.String);

            Console.Write(query);

            IEnumerable<string> existingUsers = _dapper.LoadData<string>(query);
            if (existingUsers.Count() == 0)
            {
                UserLoginDto userSettingPassword = new UserLoginDto()
                {
                    Email = user.Email,
                    Password = user.Password
                };
                if (_authHelper.SetPassword(userSettingPassword))
                {
                    string createUserQuery = $@" EXEC TutorialAppSchema.spUpsert_User
                                                @Firstname = @FirstNameParam,
                                                @LastName = @LastNameParam,
                                                @Email = @EmailParam,
                                                @Gender = @GenderParam,
                                                @Active = @ActiveParam,
                                                @Department = @DepartmentParam,
                                                @JobTitle = @JobTitleParam,
                                                @Salary = @SalaryParam";



                    dynamicParameters.Add("@FirstNameParam", user.FirstName, DbType.String);
                    dynamicParameters.Add("@LastNameParam", user.LastName, DbType.String);
                    dynamicParameters.Add("@GenderParam", user.Gender, DbType.String);
                    dynamicParameters.Add("@ActiveParam", 1, DbType.Boolean);
                    dynamicParameters.Add("@DepartmentParam", user.Department, DbType.String);
                    dynamicParameters.Add("@JobTitleParam", user.JobTitle, DbType.String);
                    dynamicParameters.Add("@SalaryParam", user.Salary, DbType.Decimal);

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

    [HttpPut("ResetPassword")]
    public IActionResult ResetPassword(UserLoginDto user)
    {
        if (_authHelper.SetPassword(user))
        {
            return Ok();
        }
        throw new Exception("Failed to update password.");
    }


    [AllowAnonymous] // Allowed to receive an anonymous request, does not require a token
    [HttpPost("Login")]
    public IActionResult Login(UserLoginDto user)
    {
        string sqlForHashAndSalt = $@"EXEC TutorialAppSchema.spGet_LoginConfirmation 
        @Email = @EmailParam";

        DynamicParameters dynamicParametersForEmail = new DynamicParameters();

        dynamicParametersForEmail.Add("@EmailParam", user.Email, DbType.String);

        UserLoginConfirmationDto userConfirmation = _dapper.
        LoadDataSingleWithParameters<UserLoginConfirmationDto>(sqlForHashAndSalt, dynamicParametersForEmail);


        byte[] passwordHash = _authHelper.CreatePasswordHash(user.Password, userConfirmation.PasswordSalt);

        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != userConfirmation.PasswordHash[i])
            {
                return StatusCode(401, "Incorrect Password.");
            }
        }

        string userIdQuery = $@"EXEC TutorialAppSchema.spGet_UserId @Email = @EmailParam";

        DynamicParameters dynamicParametersForUserId = new DynamicParameters();
        dynamicParametersForUserId.Add("@EmailParam", user.Email, DbType.String);

        int userId = _dapper.LoadDataSingleWithParameters<int>(userIdQuery, dynamicParametersForUserId);


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

        string userIdQuery = $@"EXEC TutorialAppSchema.spRefresh_Token @UserId = @UserIdParam";

        DynamicParameters dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("@UserIdParam", userIdString, DbType.String);

        int userIdNum = _dapper.LoadDataSingle<int>(userIdQuery);

        return Ok(new Dictionary<string, string> {
            {"token", _authHelper.CreateToken(userIdNum)}
        });
    }


    #endregion

}