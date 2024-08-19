namespace DotNetAPI.Controllers;

using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//[Authorize]
[ApiController]
[Route("[controller]")] // Creates the Post route for requests
public class PostController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    public PostController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    /// <summary>
    /// Get all posts in the database
    /// </summary>
    /// <returns>All posts in the database</returns>
    [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
    public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
    {
        string getQuery = "EXEC TutorialAppSchema.spGet_Posts";
        string parameters = "";

        if (postId > 0)
        {
            parameters += $", @PostId = '{postId}'";
        }

        if (userId > 0)
        {
            parameters += $", @UserId = '{userId}'";
        }

        if (searchParam.Equals("None") == false)
        {
            parameters += $", @SearchParam = '{searchParam}'";
        }

        if (parameters.Length > 0)
        {
            getQuery += parameters.Substring(1);
        }

        IEnumerable<Post> posts = _dapper.GetRows<Post>(getQuery);


        return posts;
    }

    /// <summary>
    /// Get all the posts of the user making the request
    /// </summary>
    /// <returns></returns>
    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        string? userId = this.User.FindFirst("userId")?.Value; // this keyword specifies the request is coming from PostController, not ControllerBase
        return _dapper.GetRows<Post>($"SELECT * FROM TutorialAppSchema.Posts WHERE UserId = {this.User.FindFirst("userId")?.Value}");
    }
    [HttpPost("Post")]
    public IActionResult AddPost(AddPostDto post)
    {
        string addPostQuery = $@"INSERT INTO TutorialAppSchema.Posts (
        [UserId],
        [PostTitle],
        [PostContent],
        [PostCreated],
        [LastUpdated]) VALUES (
        {this.User.FindFirst("userId")?.Value},
        '{post.PostTitle}',
        '{post.PostContent}',
        GETDATE(), GETDATE())";

        Console.WriteLine(addPostQuery);

        if (!_dapper.ExecuteSql(addPostQuery))
        {
            throw new Exception("Could not create new post.");
        }

        return Ok();
    }


    [HttpPut("Post")]
    public IActionResult EditPost(EditPostDto post)
    {
        string editPostQuery = $@"EXEC TutorialAppSchema.spUpsert_Posts
        @PostTitle = '{post.PostTitle}', 
        @PostContent = '{post.PostContent}',
        @LastUpdated = GETDATE()
        @PostId = {post.PostId} 
            WHERE UserId = {this.User.FindFirst("userId")?.Value}";

        if (!_dapper.ExecuteSql(editPostQuery))
        {
            throw new Exception("Could not edit post.");
        }

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string deleteQuery = $@"DELETE FROM TutorialAppSchema.Posts 
        WHERE PostId = {postId} AND UserId = {this.User.FindFirst("userId")?.Value}";

        if (!_dapper.ExecuteSql(deleteQuery))
        {
            throw new Exception("Could not delete post");
        }

        return Ok();
    }
}