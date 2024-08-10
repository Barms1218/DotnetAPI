using DotNetAPI.Data;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Repositories;

public class UserRepository : IUserRepository
{
    DataContextEF _entityFramework;

    /// <summary>
    /// Constructor that reads the 
    /// </summary>
    /// <param name="configuration"></param>
    public UserRepository(IConfiguration configuration)
    {
        _entityFramework = new DataContextEF(configuration);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool SaveChanges()
    {
        return _entityFramework.SaveChanges() > 0;
    }


    public void AddEntity<T>(T newEntity)
    {
        if (newEntity == null)
        {
            return;
        }

        _entityFramework.Add(newEntity);
    }

    public void RemoveEntity<T>(T newEntity)
    {
        if (newEntity == null)
        {
            return;
        }

        _entityFramework.Remove(newEntity);
    }

    public IEnumerable<User> GetUsers()
    {
        //IEnumerable<User> users = _dapper.LoadData("SELECT * FROM TutorialAppSchema.Users");
        return _entityFramework.Users.ToList<User>();
    }

    public IEnumerable<User> GetActiveUsers()
    {
        return _entityFramework.Users.Where(u => u.Active);
    }

    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users.
        Where(u => u.UserId == userId).FirstOrDefault<User>();

        if (user == null)
        {
            throw new Exception("Could not find user");
        }

        return user;
    }

    public UserJobInfo GetSingleJob(int userId)
    {
        UserJobInfo? job = _entityFramework.UserJobInfo.
        Where(j => j.UserId == userId).FirstOrDefault<UserJobInfo>();

        if (job == null)
        {
            throw new Exception("Could not find user");
        }

        return job;
    }

    public UserSalary GetSingleSalary(int userId)
    {
        UserSalary? salary = _entityFramework.UserSalary.
        Where(s => s.UserId == userId).FirstOrDefault<UserSalary>();

        if (salary == null)
        {
            throw new Exception("Could not find user");
        }

        return salary;
    }

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

        if (SaveChanges() == false)
        {
            throw new Exception("Unable to edit user.");
        }

        return Ok();
    }
}