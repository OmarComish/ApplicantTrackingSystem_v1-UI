using System.Text.Json.Serialization;

namespace ATS.API.Models;
    public class Application: BaseEntity
    {
        public int JobPostingId { get; set; }
        public int ApplicantId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
        public bool IsShortlisted { get; set; }
        public int? ShortlistRank { get; set; }
        public string CoverLetter { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? Notes { get; set; } = null;
        
        // Navigation properties
        [JsonIgnore]
        public virtual JobPosting JobPosting { get; set; }
        public virtual Applicant Applicant { get; set; }
        public virtual ICollection<ApplicationStatusHistory> StatusHistory { get; set; }
    }