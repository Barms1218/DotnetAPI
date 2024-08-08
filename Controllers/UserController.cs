using System.Net;
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

    [HttpGet("GetSalaries")]
    public IEnumerable<UserSalary> GetSalaries()
    {
        return _dapper.LoadData<UserSalary>("SELECT * FROM TutorialAppSchema.UserSalary");
    }

    [HttpGet("UserJobs")]
    public IEnumerable<UserJobInfo> GetJobs()
    {
        return _dapper.LoadData<UserJobInfo>("SELECT * FROM TutorialAppSchema.UserJobInfo");
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

    [HttpGet("GetSalary/{userId}")]
    public UserSalary GetSingleSalary(int userId)
    {
        string query = $@"SELECT * FROM TutorialAppSchema.UserSalary
                WHERE UserId = {userId}";
        return _dapper.LoadDataSingle<UserSalary>(query);
    }

    [HttpGet("GetJob/{userId}")]
    public UserJobInfo GetSingleJob(int userId)
    {
        string query = $@"SELECT * FROM TutorialAppSchema.UserJobInfo
        WHERE UserId = {userId}";

        return _dapper.LoadDataSingle<UserJobInfo>(query);
    }


    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string query = $@"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] = '{user.FirstName}',
                [LastName] = '{user.LastName}',
                [Email] = '{user.Email}',
                [Gender] = '{user.Gender}',
                [Active] = {Convert.ToInt32(user.Active)}
                    WHERE UserId = {user.UserId}";

        Console.Write(query);
        if (_dapper.ExecuteSql(query))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }

    [HttpPut("EditSalary")]
    public IActionResult EditSalary(UserSalary userSalary)
    {
        string query = $@"
        UPDATE TutorialAppSchema.UserSalary
        SET [Salary] = '{userSalary.Salary}',
        [AvgSalary] = '{userSalary.AvgSalary}'
            WHERE UserId = {userSalary.UserId}";

        Console.WriteLine(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception($"Could not find user");
        }

        return Ok();
    }

    [HttpPut("EditJob")]
    public IActionResult EditJob(UserJobInfo userJobInfo)
    {
        string query = $@"
        UPDATE TutorialAppSchema.UserJobInfo
            SET [JobTitle] = '{userJobInfo.JobTitle}',
            [Department] = '{userJobInfo.Department}'
                WHERE UserId = {userJobInfo.UserId}";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not locate user.");
        }

        return Ok();
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string query = $@"
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

        Console.Write(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Failed to add user");
        }

        return Ok();
    }

    [HttpPost("AddSalary")]
    public IActionResult AddSalary(UserSalaryDto salary)
    {
        string query = $@"
        INSERT INTO TutorialAppSchema.UserSalary 
        (
            [Salary] = {salary.Salary}m
            [AvgSalary] = {salary.AvgSalary}
        )";

        Console.WriteLine(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not add salary");
        }

        return Ok();
    }

    [HttpPost("AddJob")]
    public IActionResult AddJob(UserJobInfoDto userJobInfo)
    {
        string query = $@"Insert INTO TutorialAppSchema.UserJobInfo
        (
        [JobTitle] = '{userJobInfo.JobTitle}',
        [Department] = '{userJobInfo.Department}'
        )";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not add job information.");
        }

        return Created();
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string query = $@"DELETE FROM TutorialAppSchema.Users
            WHERE UserId = {userId}";

        Console.Write(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Failed to delete user");
        }

        return NoContent();
    }

    [HttpDelete("DeleteSalary/{userId}")]
    public IActionResult DeleteSalary(int userId)
    {
        string query = $@"DELETE FROM TutorialAppSchema.UserSalary
        WHERE UserId = {userId}";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Failed to delete user");
        }

        return NoContent();
    }

    [HttpDelete("DeleteJob/{userId}")]
    public IActionResult DeleteJob(int userId)
    {
        string query = $@"DELETE FROM TutorialAppSchema.UserJobInfo
            WHERE UserId = {userId}";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not find user.");
        }

        return NoContent();
    }
}