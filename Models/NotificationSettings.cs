namespace ATS.API.Models;
public class NotificationSettings: BaseEntity
{     
    public int UserId { get; set; }
    
    public bool EmailOnNewApplication { get; set; } = true;
    public bool EmailOnStatusChange { get; set; } = true;
    public bool EmailOnShortlist { get; set; } = true;
    
    //Navigation properties
    public virtual User User { get; set; }
}