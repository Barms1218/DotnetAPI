CREATE OR ALTER PROCEDURE TutorialAppSchema.spGet_Posts
    @UserId INT = NULL,
    @PostId INT = NULL,
    @SearchParam NVARCHAR(MAX) = NULL
AS
BEGIN
    SELECT Posts.PostId,
    Posts.UserId,
    Posts.PostTitle,
    Posts.PostContent,
    Posts.PostCreated,
    Posts.LastUpdated
        FROM TutorialAppSchema.Posts AS Posts
            WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
            AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
            AND (@SearchParam IS NULL
            OR Posts.PostContent LIKE '%' + @SearchParam + '%"'
            OR Posts.PostTitle LIKE '%' + @SearchParam + '%')
END
GO