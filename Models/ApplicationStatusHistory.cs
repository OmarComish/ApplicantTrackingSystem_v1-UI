namespace ATS.API.Models;
public class ApplicationStatusHistory: BaseEntity
{
    public int ApplicationId { get; set; }
    
    public ApplicationStatus FromStatus { get; set; }
    public ApplicationStatus ToStatus { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int ChangedByUserId { get; set; }
    
    public string Comments { get; set; }
    
    // Navigation properties
    public virtual Application Application { get; set; }
}