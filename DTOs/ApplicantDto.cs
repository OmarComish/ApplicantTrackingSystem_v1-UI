using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.Models;

namespace ATS.API.DTOs
{
    public class ApplicantDto
    {
        public int Id { get; set; }
        public string Name {get; set;}
        public string JobTitle {get; set;} 
        public decimal Score {get; set;}
        public string Education {get; set;}
        public int Experience { get; set; }
        public string Status {get; set;}
        public string Reasoning {get; set;}
        public string Email {get;set;}
    }
    public class CreateApplicantDto 
    {
        [Required]
        public Guid UserId {get; set;}
        [Required]
        public string FirstName {get; set;} 

        [Required]
        public string LastName {get; set;} 

        [Required]
        public string Email {get; set;}

        [Phone]
        public string PhoneNumber {get; set;}

        public DateTime DateofBirth { get; set; }

        [MaxLength(500)]
        public string Skills {get; set;}
        public EducationLevel EducationLevel {get; set;}
        public int YearsOfExperience { get; set; }

    }
    public class ApplicantInfoDto 
    {
        public Guid UserId {get; set;}
        public string FirstName {get; set;} 
        public string LastName {get; set;} 
        public string Email {get; set;}
        public string PhoneNumber {get; set;}
        public DateTime DateOfBirth { get; set; }
        public string Skills {get; set;}
        public EducationLevel EducationLevel {get; set;}
        public int YearsOfExperience { get; set; }

    }
    public record CreateApplicationDto1
    {
        public Guid Id {get; set;}
        public int JobPostingId {get; set;}
        [Required]
        [MaxLength(5000)]
        public string CoverLetter {get; set;}
    }

    
}