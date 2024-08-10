using System.Net;
using AutoMapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using DotNetAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserControllerEF : ControllerBase
{
    private IUserRepository _repository;
    IMapper _mapper;

    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserControllerEF(IConfiguration configuration, IUserRepository repository)
    {
        _repository = repository;

        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, UserToAddDto>();
        }));
    }


    #region User Region


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User? userDb = _mapper.Map<User>(user);

        _repository.AddEntity(userDb);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Failed to Add user.");
        }

        return Ok();
    }

    /// <summary>
    /// Get all users from the database
    /// </summary>
    /// <returns>All Users</returns>
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        //IEnumerable<User> users = _dapper.LoadData("SELECT * FROM TutorialAppSchema.Users");
        return _repository.GetUsers();
    }


    /// <summary>
    /// Get all users that are currently active
    /// </summary>
    /// <returns>All active users</returns>
    [HttpGet("GetActiveUsers")]
    public IEnumerable<User> GetActiveUsers()
    {
        return _repository.GetActiveUsers();
    }

    /// <summary>
    /// Get a single user from the database by their user id
    /// </summary>
    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _repository.GetSingleUser(userId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        if (user == null)
        {
            throw new Exception("Unable to edit user.");
        }

        User? userDb = _repository.GetSingleUser(user.UserId);


        userDb.Active = user.Active;
        userDb.FirstName = user.FirstName;
        userDb.LastName = user.LastName;
        userDb.Email = user.Email;
        userDb.Gender = user.Gender;

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Unable to edit user.");
        }

        return Ok();
    }


    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _repository.GetSingleUser(userId);

        if (userDb == null)
        {
            return NotFound();
        }

        _repository.RemoveEntity(userDb);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Failed to Delete user.");
        }

        return Ok();
    }



    #endregion

    #region Salary Region


    [HttpPost("AddSalary")]
    public IActionResult AddSalary(UserSalary newSalary)
    {
        UserSalary? salaryDb = _mapper.Map<UserSalary>(newSalary);

        _repository.AddEntity(salaryDb);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Could not find User.");
        }

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetSalaries")]
    public IEnumerable<UserSalary> GetSalaries()
    {
        return _repository.GetSalaries();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("GetSalary/{userId}")]
    public UserSalary GetSingleSalary(int userId)
    {
        return _repository.GetSingleSalary(userId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedSalary"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("EditSalary")]
    public IActionResult EditSalary(UserSalary updatedSalary)
    {
        if (updatedSalary == null)
        {
            throw new Exception("Unable to edit user.");
        }

        UserSalary? salaryDb = _repository.GetSingleSalary(updatedSalary.UserId);


        salaryDb.Salary = updatedSalary.Salary;
        salaryDb.AvgSalary = updatedSalary.AvgSalary;

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Unable to edit user.");
        }

        return Ok();
    }

    [HttpDelete("DeleteSalary/{userId}")]
    public IActionResult DeleteSalary(int userId)
    {
        UserSalary? salaryDb = _repository.GetSingleSalary(userId);

        if (salaryDb == null)
        {
            return NotFound();
        }

        _repository.RemoveEntity(salaryDb);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Failed to delete Salary.");
        }

        return NoContent();
    }

    #endregion

    #region Job Info Region

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newJobInfo"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("AddJob")]
    public IActionResult CreateJobInfo(UserJobInfoDto newJobInfo)
    {
        UserJobInfo? jobInfoDto = _mapper.Map<UserJobInfo>(newJobInfo);

        _repository.AddEntity(jobInfoDto);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Could not add job information.");
        }

        return Created();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetJobs")]
    public IEnumerable<UserJobInfo> GetJobs()
    {
        return _repository.GetJobs();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("GetSingleJob/{userId}")]
    public UserJobInfo GetSingleJob(int userId)
    {
        return _repository.GetSingleJob(userId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userJobInfo"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut("EditJob/{userId}")]
    public IActionResult EditJob(UserJobInfo userJobInfo)
    {

        if (userJobInfo == null)
        {
            return NotFound();
        }

        UserJobInfo? jobInfo = _repository.GetSingleJob(userJobInfo.UserId);

        jobInfo.JobTitle = userJobInfo.JobTitle;
        jobInfo.Department = userJobInfo.Department;

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Could not locate user.");
        }

        return Ok();
    }

    [HttpDelete("DeleteJob/{userId}")]
    public IActionResult DeleteJob(int userId)
    {
        UserJobInfo? jobInfoDb = _repository.GetSingleJob(userId);

        if (jobInfoDb == null)
        {
            return NotFound();
        }

        _repository.RemoveEntity(jobInfoDb);

        if (_repository.SaveChanges() == false)
        {
            throw new Exception("Failed to delete Salary.");
        }

        return NoContent();
    }

    #endregion
}