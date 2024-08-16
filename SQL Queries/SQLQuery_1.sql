ALTER PROCEDURE TutorialAppSchema.spGet_Users
/*EXEC TutorialAppSchema.spGet_User @UserId=3, @RunFilter=1 s*/
    @UserId INT = NULL,
    @Active BIT = NULL
AS 
BEGIN

    --DROP TABLE IF EXISTS #AverageDeptSalary -- Modern Version

    IF OBJECT_ID('tempdb..#AverageDeptSalary', 'U') IS NOT NULL
        BEGIN
            DROP TABLE #AverageDeptSalary
        END

    SELECT UserJobInfo.Department, 
    AVG(UserSalary.Salary) AvgSalary
    INTO #AverageDeptSalary -- Temporary table
    FROM TutorialAppSchema.Users AS Users  
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
            GROUP BY Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary_Department ON #AverageDeptSalary(Department) -- Create Clustered Index ON this table(using this column)

    SELECT [Users].[UserId],
    [Users].[FirstName],
    [Users].[LastName],
    [Users].[Email],
    [Users].[Gender],
    [Users].[Active],
    [UserSalary].[Salary],
    UserJobInfo.Department,
    UserJobInfo.JobTitle,
    AvgSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary AS AvgSalary
            ON AvgSalary.Department = UserJobInfo.Department
        -- OUTER APPLY (
        --         SELECT AVG(UserSalary2.Salary) AvgSalary
        --         FROM TutorialAppSchema.Users AS Users  
        --             LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary2
        --                 ON UserSalary.UserId = Users.UserId
        --             LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo2
        --                 ON UserJobInfo2.UserId = Users.UserId
        --                 WHERE UserJobInfo.Department = UserJobInfo2.Department
        --                 GROUP BY UserJobInfo2.Department
        --             ) AvgSalary
        WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
            AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END

SELECT CASE WHEN NULL = NULL THEN 1 ELSE 0 END,
    CASE WHEN NULL <> NULL THEN 1 ELSE 0 END

EXEC TutorialAppSchema.spGet_Users @Active = 1