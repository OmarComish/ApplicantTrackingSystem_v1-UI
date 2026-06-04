using System.ComponentModel.DataAnnotations;

namespace ATS.API.Models;

public class JobPosting: BaseEntity
{
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; }
        
        [MaxLength(100)]
        public string Location { get; set; }
        
        [Required]
        [MaxLength(1500)]
        public string Requirements { get; set; }
        [Required]
        [MaxLength(1500)]
        public string Responsibilities {get; set;}
        
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Open;

        [Required]
        public JobType Type { get; set; } = JobType.Contract;
        public bool Featured { get; set; }

        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; }
        public Company Company { get; set; } 
        
}