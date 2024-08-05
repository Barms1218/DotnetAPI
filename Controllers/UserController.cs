using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    public UserController(IConfiguration configuration)
    {
        Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testValue"></param>
    /// <returns></returns>
    [HttpGet("GetUsers/{testValue}")]
    public string[] GetUsers(string testValue)
    {
        string[] responseArray = new string[]
        {
            "test1",
            "test2",
            testValue
        };
        return responseArray;
    }
}