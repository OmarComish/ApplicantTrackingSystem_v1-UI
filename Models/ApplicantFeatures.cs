using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace ATS.API.Models
{
    public class ApplicantFeatures
    {
        public string ApplicantId {get; set;}
        [LoadColumn(0)]
        public float SkillMatchRatio {get; set;}
        [LoadColumn(1)]
        public float ExperienceYears { get; set; }
        [LoadColumn(2)]
        public float EducationLevel { get; set; }
        [LoadColumn(3)]
        public float KeywordDensity { get; set; }
        [LoadColumn(4)]
        public float TitleMatchScore { get; set; }
        [LoadColumn(5)]
        public float Label { get; set; } //Optional property - Useful for training when historical data is available

        //Non-ML properties
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingSkills { get; set; }
    }
}