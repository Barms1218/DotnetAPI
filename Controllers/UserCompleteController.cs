using System.Net;
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
        return _dapper.GetSingleRow<DateTime>("SELECT GETDATE()");
    }


    #region Users

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string insertQuery = $@"
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

        Console.Write(insertQuery);
        if (_dapper.ExecuteSql(insertQuery) == false)
        {
            throw new Exception("Failed to add user");
        }

        return Ok();
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

        IEnumerable<UserComplete> users = _dapper.GetRows<UserComplete>(getQuery);

        return users;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="UserComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("EditUser")]
    public IActionResult EditUser(UserComplete UserComplete)
    {
        string query = $@"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] = '{UserComplete.FirstName}',
                [LastName] = '{UserComplete.LastName}',
                [Email] = '{UserComplete.Email}',
                [Gender] = '{UserComplete.Gender}',
                [Active] = {Convert.ToInt32(UserComplete.Active)}
                    WHERE UserId = {UserComplete.UserId}";

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
        string query = $@"DELETE FROM TutorialAppSchema.Users
            WHERE UserId = {userId}";

        Console.Write(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Failed to delete user");
        }

        return NoContent();
    }

    #endregion

    #region Salaries


    /// <summary>
    /// 
    /// </summary>
    /// <param name="newSalary"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("AddSalary")]
    public IActionResult AddSalary(UserSalaryDto newSalary)
    {
        string query = $@"
        INSERT INTO TutorialAppSchema.UserSalary 
        (
            [Salary],
            [AvgSalary]
        ) VALUES
        (
            '{newSalary.Salary}',
            '{newSalary.AvgSalary}'
        )";

        Console.WriteLine(query);
        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not add salary");
        }

        return Ok(newSalary);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetSalaries")]
    public IEnumerable<UserSalary> GetSalaries()
    {
        return _dapper.GetRows<UserSalary>("SELECT * FROM TutorialAppSchema.UserSalary");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("GetSalary/{userId}")]
    public UserSalary GetSingleSalary(int userId)
    {
        string query = $@"SELECT * FROM TutorialAppSchema.UserSalary
                WHERE UserId = {userId}";
        return _dapper.GetSingleRow<UserSalary>(query);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userSalary"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

        return Ok(userSalary);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

    #endregion


    #region Jobs

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userJobInfo"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("AddJob")]
    public IActionResult AddJob(UserJobInfoDto userJobInfo)
    {
        string query = $@"Insert INTO TutorialAppSchema.UserJobInfo
        (
        [JobTitle],
        [Department]
        ) VALUES
        (
            '{userJobInfo.JobTitle}', 
            '{userJobInfo.Department}'
        )";

        if (_dapper.ExecuteSql(query) == false)
        {
            throw new Exception("Could not add job information.");
        }

        return Created();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("UserJobs")]
    public IEnumerable<UserJobInfo> GetJobs()
    {
        return _dapper.GetRows<UserJobInfo>("SELECT * FROM TutorialAppSchema.UserJobInfo");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("GetJob/{userId}")]
    public UserJobInfo GetSingleJob(int userId)
    {
        string query = $@"SELECT * FROM TutorialAppSchema.UserJobInfo
        WHERE UserId = {userId}";

        return _dapper.GetSingleRow<UserJobInfo>(query);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userJobInfo"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

        return Ok(userJobInfo);
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

    #endregion
}