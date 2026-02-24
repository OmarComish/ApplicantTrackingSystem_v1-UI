using ATS.API.DTOs;
using ATS.API.Models;
using AutoMapper;

namespace ATS.API.Mapping;
public class MappingProfiles: Profile
{
    public MappingProfiles()
    {
         CreateMap<CreateApplicantDto, Applicant>();
    }
    
}