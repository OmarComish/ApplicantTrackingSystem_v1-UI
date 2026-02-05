using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ATS.API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.ML;

namespace ATS.API.Services
{
    public class ApplicantRankingService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _modelPath;
        public ApplicantRankingService(string modelDirectory=null)
        {
            _mlContext = new MLContext(seed: 0);

            //directory for local data storage
            if(string.IsNullOrEmpty(modelDirectory))
            {
                modelDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ATS","Models");
            }
            //Create directory if it doesnt exist
            Directory.CreateDirectory(modelDirectory);
            _modelPath = Path.Combine(modelDirectory, "applicant_ranking_model.zip");

            //Try to load existing model
            if(File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var _);
                Console.WriteLine($"Model loaded from: {_modelPath}");
            }
            else
            {
                Console.WriteLine($"No existing model found. We will use rule-based scoring.");
                Console.WriteLine($"Train a model to create: {_modelPath}");
            }
        }
        ///<summary>
        /// Train the model with historical data (run this when you have labeled data)
        /// </summary>
        public void TrainModel(List<ApplicantFeatures> historicalData)
        {
            //Convert to IDataView
            var trainingData = _mlContext.Data.LoadFromEnumerable(historicalData);

            //define the pipeline
            var pipeline=_mlContext.Transforms.Concatenate("Features",
            nameof(ApplicantFeatures.SkillMatchRatio),
            nameof(ApplicantFeatures.ExperienceYears),
            nameof(ApplicantFeatures.EducationLevel),
            nameof(ApplicantFeatures.KeywordDensity),
            nameof(ApplicantFeatures.TitleMatchScore)).
            Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: nameof(ApplicantFeatures.Label),
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 10,
                learningRate: 0.2
            ));

            //Train the model
            _model = pipeline.Fit(trainingData);

            //Save the model
            _mlContext.Model.Save(_model, trainingData.Schema, _modelPath);

            Console.WriteLine($"Model trained and saved to {_modelPath}");
        }
        ///<summary>
        /// Rank applicants using ML.NET model or rule-based scoring
        /// </summary>
        public List<ApplicantScore> RankApplicants(string jobDescription, List<ResumeData> applicants)
        {
            //Step 1: Extract features
            var features = applicants.Select(a =>ExtractFeatures(jobDescription,a)).ToList();

            //Step 2: Scoe using ML.NET model or fallback to rule-based
            var scores = new List<ApplicantScore>();

            if(_model != null)
            {
                //Use ML.NET model for prediction
                scores = ScoreWithMLNodel(features);
            }
            else
            {
                //Fallback to rule-based scoring
                scores = ScoreWithRules(features);
            }
            return scores.OrderByDescending(s =>s.Score).ToList();
        }

        private List<ApplicantScore> ScoreWithRules(List<ApplicantFeatures> features)
        {
            var scores = new List<ApplicantScore>();
            foreach(var feature in features)
            {
                var score = CalculateScore(feature);
                scores.Add(new ApplicantScore
                {
                    ApplicantId = feature.ApplicantId,
                    Score = score,
                    MatchedSkills = feature.MatchedSkills,
                    MissingSkills = feature.MissingSkills,
                    ExperienceMatch = CalculateExperienceMatch(feature),
                    Reasoning = GenerateReasoning(feature, score /100.0)
                });
            }
            return scores;
        }

        private string GenerateReasoning(ApplicantFeatures features, double normalizedScore)
        {
            var reasons = new List<string>();
            if(features.SkillMatchRatio > 0.8)
               reasons.Add($"Strong skill match({features.MatchedSkills.Count} matched)");
            else if (features.SkillMatchRatio >= 0.5)
              reasons.Add($"Moderate skill match({features.MatchedSkills.Count} matched)");
            else
               reasons.Add($"Limited skill match({features.MatchedSkills.Count} matched)");

            if(features.ExperienceYears >= 5)
              reasons.Add($"Extensive experience ({features.ExperienceYears} years)");
            else if (features.ExperienceYears >= 2)
              reasons.Add($"Relevant experience ({features.ExperienceYears} years)");

            if(features.EducationLevel >= 0.8)
              reasons.Add("Strong educational background");

            if(features.TitleMatchScore >= 0.7)
              reasons.Add("Relevant job match");

            return string.Join(". ", reasons);
        }

        private double CalculateExperienceMatch(ApplicantFeatures features)
        {
            //Return a normalized experience match score
            return Math.Min(features.ExperienceYears /10.0, 1.0);
        }

        private double CalculateScore(ApplicantFeatures features)
        {
           //Weighted scoring model
           double score = 0;

           //Skill match (40%)
           score += features.SkillMatchRatio * 40;

           //Experience (30%)
           score += Math.Min(features.ExperienceYears /10.0, 1.0) * 30;

           //Education (15%)
           score += features.EducationLevel * 15;

           //Keyword density (10%)
           score += features.KeywordDensity * 10;

           //Title match (5%)
           score += features.TitleMatchScore * 5;

           return score;
        }

        private List<ApplicantScore> ScoreWithMLNodel(List<ApplicantFeatures> features)
        {
            var predictionEngine = _mlContext.
            Model.CreatePredictionEngine<ApplicantFeatures,ApplicantPrediction>(_model);

            var scores = new List<ApplicantScore>();
            foreach(var feature in features)
            {
                var prediction = predictionEngine.Predict(feature);
                scores.Add(new ApplicantScore
                {
                   ApplicantId = feature.ApplicantId,
                   Score =prediction.PredictedScore * 100, //Scale t0 0 - 100
                   MatchedSkills = feature.MatchedSkills,
                   MissingSkills = feature.MissingSkills,
                   ExperienceMatch = CalculateExperienceMatch(feature),
                   Reasoning = GenerateReasoning(feature,prediction.PredictedScore) 
                });
            }
            return scores;
        }
        private ApplicantFeatures ExtractFeatures(string jobDesc, ResumeData resume)
        {
            //Extract skills using keyword matching
            var jobSkills = ExtractSkills(jobDesc);
            var resumeSkills = ExtractSkills(resume.Text);

            var commonSkills = jobSkills.Intersect(resumeSkills,StringComparer.OrdinalIgnoreCase).ToList();
            
            //Extract Job Title for matching
            var jobTitle = ExtractJobTitle(jobDesc);
            var resumeTitles = ExtractTitles(resume.Text);

            return new ApplicantFeatures
            {
                ApplicantId = resume.Id,
                SkillMatchRatio = jobSkills.Count > 0 ? (float)commonSkills.Count /jobSkills.Count: 0f,
                MatchedSkills = commonSkills,
                MissingSkills = jobSkills.Except(resumeSkills,StringComparer.OrdinalIgnoreCase).ToList(),
                ExperienceYears = ExtractExperienceYears(resume.Text),
                EducationLevel = ExtractEducationLevel(resume.Text),
                KeywordDensity = CalculateKeywordDensity(jobDesc, resume.Text),
                TitleMatchScore = CalculateTitleMatch(jobTitle, resumeTitles)
            };
        }

        #region Helper Methods
        private List<string> ExtractSkills(string text)
        {
            //Common technical skills - expand this list based on your domain
            var skillsDatabase = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
               "C#",".NET","ASP.NET","SQL","JavaScript","React","Angular","Python","Java","Azure",
               "AWS","Docker","Kebernetes","Machine Learning", "AI","REST API","Microservices",
               "Agile", "Scrum", "Git", "CI/CD", "DevOps","TypeScript" 
            };
            var foundSkills = new List<string>();
            var lowerText = text.ToLower();

            foreach(var skill in skillsDatabase)
            {
                if(lowerText.Contains(skill.ToLower()))
                {
                    foundSkills.Add(skill);
                }
            }
            return foundSkills;
        }
        private float ExtractExperienceYears(string resumeText)
        {
           //Simple regex pattern tofind  "X years or X+ years"
           var pattern = @"(\d+)[\+]?\s*(?:years?|yrs?)";
           var matches = System.Text.RegularExpressions.Regex.Matches(
            resumeText,pattern,System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if(matches.Count > 0)
            {
                var years = matches.Select(m =>int.Parse(m.Groups[1].Value)).Max();
                return years;
            }
            return 0f;
        }
        private float ExtractEducationLevel(string resumText)
        {
            var text = resumText.ToLower();
            if(text.Contains("phd") || text.Contains("ph.d") || text.Contains("doctorate"))
              return 1.0f;
            
            if(text.Contains("master") || text.Contains("mba") || text.Contains("m.s"))
              return 0.8f;
            
            if(text.Contains("bachelor") || text.Contains("b.s") || text.Contains("b.a"))
              return 0.6f;

            if(text.Contains("associate")  || text.Contains("diploma"))
              return 0.4f;

            return 0.2f; //High school diploma or unspecified
        }
        private float CalculateKeywordDensity(string jobDesc, string resumeText)
        {
            var jobWords = jobDesc.ToLower()
            .Split(new[] {' ', ',', '.', ';', '\n'},
            StringSplitOptions.RemoveEmptyEntries).Where(w =>w.Length > 3)
            .Distinct()
            .ToList();

            var resumeWords = resumeText.ToLower()
            .Split(new[] {' ', ',', '.', ';', '\n'},
            StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            var matchCount = jobWords.Count(w => resumeWords.Contains(w));

            return jobWords.Count > 0? (float)matchCount /jobWords.Count: 0f;
        } 
        private string ExtractJobTitle(string jobDesc)
        {
            //Look for common patterns
            var lines = jobDesc.Split('\n');
            return lines.FirstOrDefault()?.Trim()??"";
        }
        
        private List<string> ExtractTitles(string resumeText)
        {
           //Extract potential job titles from resume
           //This is simplified 
           var titles = new List<string>();
           var lines = resumeText.Split('\n');

           //Look for lines that might be job titles
            foreach(var line in lines)
            {
                if(line.Contains("Engineer") || line.Contains("Developer") || 
                line.Contains("Manager") || line.Contains("Analyst"))
                {
                    titles.Add(line.Trim());
                }
            }
            return titles;
        }
        private float CalculateTitleMatch(string jobTitle, List<string> resumeTitles)
        {
           if(string.IsNullOrEmpty(jobTitle) || !resumeTitles.Any())
             return 0f;

            var jobTitleWords = jobTitle.ToLower().Split(' ');

            var maxMatch = 0f;
            foreach(var resumeTitle in resumeTitles)
            {
                var resumeTitleWords = resumeTitle.ToLower().Split(' ');
                var commonWords = jobTitleWords.Intersect(resumeTitleWords).Count();
                var match = (float)commonWords / jobTitleWords.Length;
                if(match > maxMatch)
                 maxMatch = match;
            }
            return maxMatch;
        }

        #endregion
    }
}