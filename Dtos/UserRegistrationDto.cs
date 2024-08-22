namespace DotNetAPI.Dtos;

public partial class UserRegistrationDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PassWordConfirm { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Gender { get; set; } = "";
    public bool Active { get; set; };
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
    public Decimal Salary { get; set; }
}