namespace DotNetAPI.Dtos;

public partial class UserForRegistrationDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PassWordConfirm { get; set; } = "";
}