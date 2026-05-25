using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.DTOs
{
    public class ReadJobPostingDto
    {
        public int Id { get; set; }
        public string Title {get; set;} = string.Empty;
        public string Department {get; set;} = string.Empty;
        public string Location {get; set;} = string.Empty;
        public int Applicants { get; set; } 
        public string Status { get; set; } = string.Empty;
    }
}