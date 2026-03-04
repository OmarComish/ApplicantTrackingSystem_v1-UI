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
        public string FirstName {get; set;} 
        public string LastName {get; set;} 
        public string Email {get; set;}
        public string PhoneNumber {get; set;}
        public DateTime DateofBirth { get; set; }

        [MaxLength(500)]
        public string Skills {get; set;}
        public EducationLevel EducationLevel {get; set;}
    }
    public class CreateApplicantDto 
    {
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
}