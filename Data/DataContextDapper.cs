namespace DotNetAPI.Data;

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;


/// <summary>
/// Class contaning methods that allow manipulation and access of the SQL database
/// </summary>
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

    /// <summary>
    /// Returns whether or not the sql affected any change
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public bool ExecuteSql(string sql)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        return dbConnection.Execute(sql) > 0;
    }

    /// <summary>
    /// Returns the number of rows affected
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public int ExecuteSqlWithRowCount(string sql)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        return dbConnection.Execute(sql);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public bool ExecuteSqlWithParameters(string query, List<SqlParameter> parameters)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        SqlCommand commandWithParams = new SqlCommand(query);

        foreach (SqlParameter param in parameters)
        {
            commandWithParams.Parameters.Add(param);
        }

        dbConnection.Open();

        commandWithParams.Connection = (SqlConnection)dbConnection;

        int rowsAffected = commandWithParams.ExecuteNonQuery(); // Get the number of rows affected

        dbConnection.Close();

        return rowsAffected > 0;
    }


    /// <summary>
    /// Method that abstracts a Query operation and returns all rows
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters sqlParameters)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        return dbConnection.Query<T>(sql, sqlParameters);
    }

    /// <summary>
    /// Method that abstracts the query operation and runs a single row query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters sqlParameters)
    {
        IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        Console.WriteLine(sql);

        Console.WriteLine(sqlParameters);

        return dbConnection.QuerySingle<T>(sql, sqlParameters);
    }
}


