CREATE OR ALTER PROCEDURE TutorialAppSchema.spGet_Posts
/* EXEC TutorialAppSchema.spGet_Posts */
    @UserId INT = NULL,
    @SearchValue NVARCHAR(MAX) = NULL,
    @PostId INT = NULL
AS
BEGIN
    SELECT [POSTS].[PostId],
    [POSTS].[UserId],
    [POSTS].[PostTitle],
    [POSTS].[PostContent],
    [POSTS].[PostCreated],
    [POSTS].[LastUpdated] FROM TutorialAppSchema.Posts AS Posts
        WHERE UserId = ISNULL(@UserId, Posts.UserId)
            AND PostId = ISNULL(@PostId, Posts.PostId)
            AND (@SearchValue IS NULL
            OR Posts.PostContent LIKE '%' + @SearchValue + '%'
            OR Posts.PostTitle LIKE '%' + @SearchValue + '%')
END

EXEC TutorialAppSchema.spGet_Posts @PostId = 9