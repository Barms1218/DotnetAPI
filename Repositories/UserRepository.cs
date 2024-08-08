using DotNetAPI.Data;

namespace DotNetAPI.Repositories;

public class UserRepository
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
}