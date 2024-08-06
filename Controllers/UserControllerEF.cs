using System.Net;
using AutoMapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserControllerEF : ControllerBase
{
    DataContextEF _entityFramework;
    IMapper _mapper;
    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserControllerEF(IConfiguration configuration)
    {
        _entityFramework = new DataContextEF(configuration);

        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, UserToAddDto>();
        }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    // [HttpGet("TestConnection")]
    // public DateTime TestConnection()
    // {
    //     return _entityFramework.GetService(typeof(DateTime)) as DateTime;
    //     //return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    // }

    /// <summary>
    /// Get all users from the database
    /// </summary>
    /// <returns>All Users</returns>
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        //IEnumerable<User> users = _dapper.LoadData("SELECT * FROM TutorialAppSchema.Users");
        return _entityFramework.Users.ToList<User>();
    }

    /// <summary>
    /// Get all users that are currently active
    /// </summary>
    /// <returns>All active users</returns>
    [HttpGet("GetActiveUsers")]
    public IEnumerable<User> GetActiveUsers()
    {
        return _entityFramework.Users.Where(u => u.Active);
    }

    /// <summary>
    /// Get a single user from the database by their user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>The User from the database</returns>
    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();

        if (user == null)
        {
            throw new Exception("Could not find user");
        }

        return user;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        if (user == null)
        {
            throw new Exception("Unable to edit user.");
        }

        User? userDb = _entityFramework.Users.Where(u => u.UserId == user.UserId).FirstOrDefault<User>();


        userDb.Active = user.Active;
        userDb.FirstName = user.FirstName;
        userDb.LastName = user.LastName;
        userDb.Email = user.Email;
        userDb.Gender = user.Gender;

        if (_entityFramework.SaveChanges() <= 0)
        {
            throw new Exception("Unable to edit user.");
        }

        return Ok();
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User? userDb = _mapper.Map<User>(user);

        _entityFramework.Add(userDb);

        if (_entityFramework.SaveChanges() <= 0)
        {
            throw new Exception("Failed to Add user.");
        }

        return Ok();
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault<User>();

        if (userDb == null)
        {
            return NotFound();
        }

        _entityFramework.Remove(userDb);

        if (_entityFramework.SaveChanges() <= 0)
        {
            throw new Exception("Failed to Delete user.");
        }

        return Ok();
    }
}