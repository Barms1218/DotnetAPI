namespace DotNetAPI.Controllers;

using DotNetAPI.Data;
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
        return _dapper.GetRows<Post>($"SELECT * FROM TutorialAppSchema.Posts WHERE UserId = {userId}");
    }
}