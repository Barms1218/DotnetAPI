using System.Net;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;

    private DynamicParameters _dynamicParams;

    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserCompleteController(IConfiguration configuration)
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
    public IEnumerable<UserComplete> GetUsers(int userId = 0, bool isActive = false)
    {
        string getQuery = $"EXEC TutorialAppSchema.spGet_Users ";
        string parameters = "";

        if (userId != 0)
        {
            parameters += $", @UserId= '{userId}'";
        }

        if (isActive)
        {
            parameters += $", @Active = {Convert.ToBoolean(isActive)}";
        }

        getQuery += parameters.Substring(1);

        Console.WriteLine(getQuery);

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(getQuery);

        return users;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="UserComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("Upsert")]
    public IActionResult UpsertUser(UserComplete UserComplete)
    {
        string query = $@" EXEC TutorialAppSchema.spUpsert_User
                @Firstname = '{UserComplete.FirstName}',
                @LastName = '{UserComplete.LastName}',
                @Email = '{UserComplete.Email}',
                @Gender = '{UserComplete.Gender}',
                @Active = {Convert.ToInt32(UserComplete.Active)},
                @Department = '{UserComplete.Department}',
                @JobTitle = '{UserComplete.JobTitle}',
                @Salary = '{UserComplete.Salary}',
                @UserId = {UserComplete.UserId}";

        Console.Write(query);
        if (_dapper.ExecuteSql(query))
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
        string query = $@"EXEC TutorialAppSchema.spDelete_User @UserId = {userId}";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Failed to delete user");
        }

        return Ok();
    }
}