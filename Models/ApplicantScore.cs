using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.Models
{
    public class ApplicantScore
    {
        public string  ApplicantId { get; set; }
        public double Score { get; set; }
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingSkills { get; set; }
        public double ExperienceMatch { get; set; }
        public string Reasoning  { get; set; }
    }
}