namespace DotNetAPI.Controllers;

using System.Data;
using Dapper;
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

    private dynamic _dynamicParams;
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

        if (searchParam.ToLower().Equals("none") == false)
        {
            parameters += $", @SearchParam = '{searchParam}'";
        }

        if (parameters.Length > 0)
        {
            getQuery += parameters.Substring(1);
        }

        IEnumerable<Post> posts = _dapper.LoadData<Post>(getQuery);


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
        Console.Write(userId);
        return _dapper.LoadData<Post>($@"EXEC TutorialAppSchema.spGet_Posts
            @UserId = {this.User.FindFirst("userId")?.Value}");
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPost(Post post)
    {
        string addPostQuery = $@"EXEC TutorialAppSchema.spUpsert_Posts
        @UserId = {this.User.FindFirst("userId")?.Value},
        @PostTitle = @PostTitleParam,
        @PostContent = @PostContentParam";

        _dynamicParams = new DynamicParameters();

        _dynamicParams.Add("@PostTitleParam", post.PostTitle, DbType.String);
        _dynamicParams.Add("@PostContentParam", post.PostContent, DbType.String);

        if (post.PostId > 0)
        {
            addPostQuery += $", @PostId = {post.PostId}";
        }

        Console.WriteLine(addPostQuery);

        if (!_dapper.ExecuteSql(addPostQuery))
        {
            throw new Exception("Could not upsert post.");
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
        string deleteQuery = $@"EXEC TutorialAppSchema.spDelete_Post
        @PostId = @PostIdParam,
        @UserId = {this.User.FindFirst("userId")?.Value}";

        _dynamicParams = new DynamicParameters();

        _dynamicParams.Add("@PostIdParam", postId, DbType.String);

        if (!_dapper.ExecuteSqlWithParameters(deleteQuery, _dynamicParams))
        {
            throw new Exception("Could not delete post");
        }

        return Ok();
    }
}