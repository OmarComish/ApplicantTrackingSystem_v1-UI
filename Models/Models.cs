using System.ComponentModel.DataAnnotations;

namespace ATS.API.Models
{
    public class JobPosting
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; }
        
        [MaxLength(100)]
        public string Location { get; set; }
        
        [Required]
        public string Requirements { get; set; }
        
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Open;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        public int CreatedByUserId { get; set; }
        
        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; }
    }

    public class Applicant
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        public string ResumeUrl { get; set; }
        
        [Required]
        public EducationLevel EducationLevel { get; set; }
        
        public int YearsOfExperience { get; set; }
        
        public string Skills { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; }
    }

    public class Application
    {
        public int Id { get; set; }
        
        public int JobPostingId { get; set; }
        public int ApplicantId { get; set; }
        
        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
        
        public bool IsShortlisted { get; set; }
        public int? ShortlistRank { get; set; }
        
        public string CoverLetter { get; set; }
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StatusUpdatedAt { get; set; }
        
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual JobPosting JobPosting { get; set; }
        public virtual Applicant Applicant { get; set; }
        public virtual ICollection<ApplicationStatusHistory> StatusHistory { get; set; }
    }

    public class ApplicationStatusHistory
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        
        public ApplicationStatus FromStatus { get; set; }
        public ApplicationStatus ToStatus { get; set; }
        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public int ChangedByUserId { get; set; }
        
        public string Comments { get; set; }
        
        // Navigation properties
        public virtual Application Application { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        public bool EmailOnNewApplication { get; set; } = true;
        public bool EmailOnStatusChange { get; set; } = true;
        public bool EmailOnShortlist { get; set; } = true;
        
        public virtual User User { get; set; }
    }

    // Enums
    public enum JobStatus
    {
        Draft,
        Open,
        Closed,
        Archived
    }

    public enum ApplicationStatus
    {
        New,
        Reviewing,
        Shortlisted,
        Interviewing,
        Offered,
        Rejected,
        Withdrawn,
        Hired
    }

    public enum EducationLevel
    {
        HighSchool,
        Associate,
        Bachelor,
        Master,
        Doctorate,
        Other
    }

    public enum UserRole
    {
        HRUser,
        HRAdmin,
        Recruiter
    }
}
