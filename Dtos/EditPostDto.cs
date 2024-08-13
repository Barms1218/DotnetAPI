namespace DotNetAPI.Dtos;

public partial class EditPostDto
{
    public int PostId { get; set; }
    public string PostTitle { get; set; } = "";
    public string PostContent { get; set; } = "";
}