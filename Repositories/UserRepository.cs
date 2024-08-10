using DotNetAPI.Data;
using DotNetAPI.Models;

namespace DotNetAPI.Repositories;

public class UserRepository : IUserRepository
{
    private DataContextEF _entityFramework;

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
    public bool SaveChanges() => _entityFramework.SaveChanges() > 0;


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

    public IEnumerable<User> GetUsers() => _entityFramework.Users.ToList<User>();

    public IEnumerable<UserSalary> GetSalaries() => _entityFramework.UserSalary.ToList<UserSalary>();

    public IEnumerable<UserJobInfo> GetJobs() => _entityFramework.UserJobInfo.ToList<UserJobInfo>();

    public IEnumerable<User> GetActiveUsers() => _entityFramework.Users.Where(u => u.Active);

    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users.
        Where(u => u.UserId == userId).FirstOrDefault<User>();

        return user ?? throw new Exception("Could not find user");
    }

    public UserJobInfo GetSingleJob(int userId)
    {
        UserJobInfo? job = _entityFramework.UserJobInfo.
        Where(j => j.UserId == userId).FirstOrDefault<UserJobInfo>();

        return job ?? throw new Exception("Could not find user");
    }

    public UserSalary GetSingleSalary(int userId)
    {
        UserSalary? salary = _entityFramework.UserSalary.
        Where(s => s.UserId == userId).FirstOrDefault<UserSalary>();

        return salary ?? throw new Exception("Could not find user");
    }
}