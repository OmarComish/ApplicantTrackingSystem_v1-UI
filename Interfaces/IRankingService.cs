using ATS.API.Models;

namespace ATS.API.Interfaces;
public interface IRankingService
{
    void TrainModel(List<ApplicantFeatures> historicalData);
    Task<List<ApplicantScore>> RankApplicants(string jobDescription, List<ResumeData> applicants);
}