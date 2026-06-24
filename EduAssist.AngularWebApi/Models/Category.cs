namespace EduAssist.AngularWebApi.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string SubjectName { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<UserRequest> UserRequests { get; set; } = new List<UserRequest>();
}
