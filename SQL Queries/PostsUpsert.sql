CREATE PROCEDURE TutorialAppSchema.spUpsert_Posts
    @UserId INT,
    @PostTitle NVARCHAR(255),
    @PostContent NVARCHAR(MAX),
    @PostId INT = NULL
AS
BEGIN
    IF NOT EXISTS(SELECT * FROM TutorialAppSchema.Posts WHERE PostId = @PostId)
        BEGIN
            INSERT INTO TutorialAppSchema.Posts (
                UserId,
                PostTitle,
                PostContent,
                PostCreated,
                LastUpdated
            ) VALUES (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE
        BEGIN
            UPDATE TutorialAppSchema.Posts
            SET PostTitle = @PostTitle,
            PostContent = @PostContent,
            LastUpdated = GetDate()
                WHERE PostId = @PostId
        END
END
