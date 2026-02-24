using ATS.API.DTOs;
using ATS.API.Interfaces;
using ATS.API.Models;

namespace ATS.API.Services;
public class JobPostingServicebkp: IJobPostingServicebkp
{
    public async Task<ResponseDto> CreateJobPosting(CreateJobPostingDto createjobPostingDto)
    {
        var response = new ResponseDto {Status = "error"};
        if(createjobPostingDto == null)
        {
              response.Message = "Null or Invalid job posting data";
             return response;
        }
        
        return response;
    }
}