using System.Data;
using System.Net;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly DataContextDapper _dapper;


    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    /// <summary>
    /// Method to request either all users or a single user
    /// </summary>
    /// <param name="userId">Default to 0, change for specific user</param>
    /// <returns></returns>
    [HttpGet("GetUsers/{userId}/{Active}")]
    public IEnumerable<User> GetUsers(int userId = 0, bool isActive = false)
    {
        string getQuery = "EXEC TutorialAppSchema.spGet_Users ";
        string parameters = "";

        DynamicParameters dynamicParams = new DynamicParameters();

        dynamicParams = new DynamicParameters();

        if (userId != 0)
        {
            dynamicParams.Add("@UserIdParam", userId, DbType.Int32);
            parameters += ", @UserId= @UserIdParam";
        }

        if (isActive)
        {
            dynamicParams.Add("@ActiveParam", isActive, DbType.Boolean);
            parameters += ", @Active = @ActiveParam";
        }

        getQuery += parameters.Substring(1);

        Console.WriteLine(getQuery);

        IEnumerable<User> users = _dapper.LoadData<User>(getQuery);

        return users;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="User"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("Upsert")]
    public IActionResult UpsertUser(User user)
    {
        DynamicParameters dynamicParams = new DynamicParameters();
        dynamicParams.Add("@FirstNameParam", user.FirstName, DbType.String);
        dynamicParams.Add("@LastNameParam", user.LastName, DbType.String);
        dynamicParams.Add("@EmailParam", user.Email, DbType.String);
        dynamicParams.Add("@GenderParam", user.Gender, DbType.String);
        dynamicParams.Add("@ActiveParam", user.Active, DbType.Boolean);
        dynamicParams.Add("@DepartmentParam", user.Department, DbType.String);
        dynamicParams.Add("@JobTitleParam", user.JobTitle, DbType.String);
        dynamicParams.Add("@SalaryParam", user.Salary, DbType.Decimal);
        dynamicParams.Add("@UserIdParam", user.UserId, DbType.Int32);

        string query = @" EXEC TutorialAppSchema.spUpsert_User
                @Firstname = @FirstNameParam,
                @LastName = @LastNameParam,
                @Email = @EmailParam,
                @Gender = @GenderParam,
                @Active = @ActiveParam,
                @Department = @DepartmentParam,
                @JobTitle = @JobTitleParam,
                @Salary = @Salaryparam,
                @UserId = @UserIdParam";

        Console.Write(query);
        if (_dapper.ExecuteSqlWithParameters(query, dynamicParams))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string query = "EXEC TutorialAppSchema.spDelete_User @UserId = @UserIdParam";

        DynamicParameters dynamicParams = new DynamicParameters();

        dynamicParams.Add("@UserIdParam", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameters(query, dynamicParams) == false)
        {
            throw new Exception("Failed to delete user");
        }

        return Ok();
    }
}