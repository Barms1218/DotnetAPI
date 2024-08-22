namespace DotNetAPI.Models;

public partial class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public bool Active { get; set; } = false;
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
    public Decimal Salary { get; set; }
    public Decimal AvgSalary { get; set; }
}