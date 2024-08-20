CREATE OR ALTER PROCEDURE TutorialAppSchema.spVerify_User
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT * FROM TutorialAppSchema.Users WHERE Email = @Email
END