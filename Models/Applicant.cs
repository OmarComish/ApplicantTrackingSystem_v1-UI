using System.ComponentModel.DataAnnotations;

namespace ATS.API.Models;
    public class Applicant: BaseEntity
    {
        [Required]
        public Guid UserId {get; set;}
        
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
        
        public string? ResumeUrl { get; set; }
        
        [Required]
        public EducationLevel EducationLevel { get; set; }
        [Required]
        public DateTime DateOfBirth {get; set;}
        
        public int YearsOfExperience { get; set; }
        
        public string Skills { get; set; }
       
        
        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; }
    }