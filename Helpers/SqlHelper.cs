using System.Data;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Models;

namespace DotNetAPI.Helpers;

public class SqlHelper
{
    private readonly DataContextDapper _dapper;

    public SqlHelper(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }

    public bool UpsertUser(User user)
    {
        DynamicParameters dynamicParams = new DynamicParameters();

        dynamicParams.Add("@FirstNameParam", user.FirstName, DbType.String);
        dynamicParams.Add("@LastNameParam", user.LastName, DbType.String);
        dynamicParams.Add("@EmailParam", user.Email, DbType.String);
        dynamicParams.Add("@GenderParam", user.Gender, DbType.String);
        dynamicParams.Add("@ActiveParam", user.Active, DbType.Boolean);
        dynamicParams.Add("@DepartmentParam", user.Department, DbType.String);
        dynamicParams.Add("@JobTitleParam", user.JobTitle, DbType.String);
        dynamicParams.Add("@SalaryParam", user.Salary, DbType.Decimal);
        dynamicParams.Add("@UserIdParam", user.UserId, DbType.Int32);

        string query = @" EXEC TutorialAppSchema.spUpsert_User
                @Firstname = @FirstNameParam,
                @LastName = @LastNameParam,
                @Email = @EmailParam,
                @Gender = @GenderParam,
                @Active = @ActiveParam,
                @Department = @DepartmentParam,
                @JobTitle = @JobTitleParam,
                @Salary = @Salaryparam,
                @UserId = @UserIdParam";

        return _dapper.ExecuteSqlWithParameters(query, dynamicParams);
    }
}