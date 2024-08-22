using System.Data;
using System.Net;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Helpers;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly SqlHelper _sqlHelper;

    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _sqlHelper = new SqlHelper(configuration);
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
    [HttpGet("GetUsers/{userId}/{active}")]
    public IEnumerable<User> GetUsers(int userId = 0, bool active = false)
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

        if (active)
        {
            dynamicParams.Add("@ActiveParam", active, DbType.Boolean);
            parameters += ", @Active = @ActiveParam";
        }

        if (parameters.Length > 0)
        {
            getQuery += parameters.Substring(1);
        }

        IEnumerable<User> users = _dapper.LoadDataWithParameters<User>(getQuery, dynamicParams);

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
        if (_sqlHelper.UpsertUser(user))
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