using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
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
    /// Get all users from the database
    /// </summary>
    /// <returns>All Users</returns>
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        //IEnumerable<User> users = _dapper.LoadData("SELECT * FROM TutorialAppSchema.Users");
        return _dapper.LoadData<User>("SELECT * FROM TutorialAppSchema.Users");
    }

    /// <summary>
    /// Get all users that are currently active
    /// </summary>
    /// <returns>All active users</returns>
    [HttpGet("GetActiveUsers")]
    public IEnumerable<User> GetActiveUsers()
    {
        string query = @"SELECT * FROM TutorialAppSchema.Users 
        WHERE Active = 1";
        return _dapper.LoadData<User>(query);
    }

    /// <summary>
    /// Get a single user from the database by their user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>The User from the database</returns>
    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string query = $@"SELECT * FROM TutorialAppSchema.Users 
        WHERE UserId = {userId}";
        return _dapper.LoadDataSingle<User>(query);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = $@"
        UPDATE TutorialAppSchema.Users
            SET [FirstName] = '{user.FirstName}',
            [LastName] = '{user.LastName}',
            [Email] = '{user.Email}',
            [Gender] = '{user.Gender}',
            [Active] = {Convert.ToInt32(user.Active)}
                WHERE UserId = {user.UserId}";

        Console.Write(sql);
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserDto user)
    {
        string sql = $@"
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
            {Convert.ToInt32(user.Active)}
        )";
        Console.Write(sql);
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to add user");
    }
}