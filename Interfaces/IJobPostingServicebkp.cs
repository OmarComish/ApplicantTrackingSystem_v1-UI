using ATS.API.DTOs;
using ATS.API.Models;

namespace ATS.API.Interfaces;
public interface IJobPostingServicebkp
{
    Task<ResponseDto> CreateJobPosting(CreateJobPostingDto createjobpostingdto);
}