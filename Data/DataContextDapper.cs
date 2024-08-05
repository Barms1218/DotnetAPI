using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotNetAPI.Data;

class DataContextDapper
{
    private readonly IConfiguration _config;

    public DataContextDapper(IConfiguration configuration)
    {
        _config = configuration;
    }
    
    /// <summary>
    /// Method that abstracts a Query operation and returns all rows
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public IEnumerable<T> LoadData<T>(string sql)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        return dbConnection.Query<T>(sql);
    }

    /// <summary>
    /// Method that abstracts the query operation and runs a single row query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public T LoadDataSingle<T>(string sql)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        return dbConnection.QuerySingle<T>(sql);
    }
}


