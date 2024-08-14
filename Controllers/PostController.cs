namespace DotNetAPI.Controllers;

using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
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
    [HttpGet("Posts")]
    public IEnumerable<Post> GetPosts()
    {
        return _dapper.GetRows<Post>("SELECT * FROM TutorialAppSchema.Posts");
    }

    /// <summary>
    /// Get a single post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [HttpGet("SinglePost/{postId}")]
    public Post GetSinglePost(int postId)
    {
        return _dapper.GetSingleRow<Post>($"SELECT * FROM TutorialAppSchema.Posts WHERE PostId = {postId}");
    }

    /// <summary>
    /// Get all posts made by a specific user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("PostsByUser/{userId}")]
    public IEnumerable<Post> GetPostsByUser(int userId)
    {
        return _dapper.GetRows<Post>($"SELECT * FROM TutorialAppSchema.Posts WHERE UserId = {userId}");
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

    /// <summary>
    /// Get all the posts with a title or content containing the parameter
    /// </summary>
    /// <returns></returns>
    [HttpGet("SearchByPost/{searchParam}")]
    public IEnumerable<Post> GetByPost(string searchParam)
    {
        string? searchByPostQuery = $@"SELECT * FROM TutorialAppSchema.Posts 
        WHERE PostTitle LIKE '%{searchParam}%' OR PostContent LIKE '%{searchParam}%'";
        return _dapper.GetRows<Post>(searchByPostQuery);
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
        string editPostQuery = $@"UPDATE TutorialAppSchema.Posts 
        SET PostTitle = '{post.PostTitle}', 
        PostContent = '{post.PostContent}',
        LastUpdated = GETDATE()
        WHERE PostId = {post.PostId} AND UserId = {this.User.FindFirst("userId")?.Value}";

        Console.WriteLine(editPostQuery);

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