
namespace DotNetAPI.Models;

public partial class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public bool Active { get; set; } = false;

    internal object Where(Func<object, bool> value)
    {
        throw new NotImplementedException();
    }
}