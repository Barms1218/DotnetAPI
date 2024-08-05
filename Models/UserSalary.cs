namespace DotNetAPI.Models;

public partial class UserSalary
{
    public int UserId { get; set; }
    public Decimal Salary { get; set; }
    public Decimal AvgSalary { get; set; }
}