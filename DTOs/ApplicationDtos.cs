using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.Models;

namespace ATS.API.DTOs
{
    public class ApplicationResponseDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string Location { get; set; }
        public string Company { get; set; } 
        public  string Status { get; set; }
        public DateTime AppliedDate {get; set;}
        
    }
}