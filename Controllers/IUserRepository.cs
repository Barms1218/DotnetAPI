using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Data;

public interface IUserRepository
{
    public bool SaveChanges();

    public void AddEntity<T>(T EntityToAdd);

    public void RemoveEntity<T>(T EntityToRemove);

    public IEnumerable<User> GetUsers();

    public IEnumerable<UserSalary> GetSalaries();

    public IEnumerable<UserJobInfo> GetJobs();

    public IEnumerable<User> GetActiveUsers();

    public User GetSingleUser(int userId);

    public UserJobInfo GetSingleJob(int userId);

    public UserSalary GetSingleSalary(int userId);
}