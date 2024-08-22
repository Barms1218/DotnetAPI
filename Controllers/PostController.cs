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

    public PostController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    /// <summary>
    /// Get all posts in the database
    /// </summary>
    /// <returns>All posts in the database</returns>
    [HttpGet("Posts/{postId}/{userId}/{searchValue}")]
    public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchValue = "None")
    {
        string getQuery = "EXEC TutorialAppSchema.spGet_Posts";
        string parameters = "";

        DynamicParameters dynamicParams = new DynamicParameters();

        if (postId > 0)
        {
            parameters += ", @PostId = @PostIdParam";
            dynamicParams.Add("@PostIdParam", postId, DbType.Int32);
        }

        if (userId > 0)
        {
            parameters += ", @UserId = @UserIdParam";
            dynamicParams.Add("@UserIdParam", userId, DbType.Int32);
        }

        if (searchValue.ToLower().Equals("none") == false)
        {
            parameters += $", @SearchValue = @SearchParam";
            dynamicParams.Add("SearchParam", postId, DbType.Int32);
        }

        if (parameters.Length > 0)
        {
            getQuery += parameters.Substring(1);
        }

        IEnumerable<Post> posts = _dapper.LoadDataWithParameters<Post>(getQuery, dynamicParams);


        return posts;
    }

    /// <summary>
    /// Get all the posts of the user making the request
    /// </summary>
    /// <returns></returns>
    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        string myPostsQuery = "EXEC TutorialAppSchema.spGet_Posts @UserId = @UserIdparam";
        string? userId = this.User.FindFirst("userId")?.Value; // this keyword specifies the request is coming from PostController, not ControllerBase

        DynamicParameters dynamicParams = new DynamicParameters();
        dynamicParams.Add("@UserIdParam", userId, DbType.String);

        return _dapper.LoadDataWithParameters<Post>(myPostsQuery, dynamicParams);
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPost(Post post)
    {
        string addPostQuery = $@"EXEC TutorialAppSchema.spUpsert_Posts
        @UserId = @UserIdParam,
        @PostTitle = @PostTitleParam,
        @PostContent = @PostContentParam";

        DynamicParameters dynamicParams = new DynamicParameters();

        dynamicParams.Add("@PostTitleParam", post.PostTitle, DbType.String);
        dynamicParams.Add("@PostContentParam", post.PostContent, DbType.String);
        dynamicParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.String);

        if (post.PostId > 0)
        {
            addPostQuery += $", @PostId = @PostIdParam";
            dynamicParams.Add("@PostIdParam", post.PostId, DbType.Int32);
        }

        Console.WriteLine(addPostQuery);

        if (!_dapper.ExecuteSqlWithParameters(addPostQuery, dynamicParams))
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
        string deleteQuery = @"EXEC TutorialAppSchema.spDelete_Post
        @PostId = @PostIdParam,
        @UserId = @UserIdParam";

        DynamicParameters dynamicParams = new DynamicParameters();

        dynamicParams.Add("@PostIdParam", postId, DbType.String);
        dynamicParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.String);

        if (!_dapper.ExecuteSqlWithParameters(deleteQuery, dynamicParams))
        {
            throw new Exception("Could not delete post");
        }

        return Ok();
    }
}