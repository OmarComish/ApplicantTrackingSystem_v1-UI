using ATS.API.DTOs;
using ATS.API.Models;
using AutoMapper;

namespace ATS.API.Mapping;
public class MappingProfiles: Profile
{
    public MappingProfiles()
    {
         CreateMap<CreateApplicantDto, Applicant>();
         CreateMap<CreateCompanyDto, Company>();
         CreateMap<Company, CompanyDto>()
         .ForMember(dest => dest.IndustryName, opt => opt.MapFrom(src => src.Industry.Name));

          CreateMap<Applicant, ApplicantInfoDto>()
            .ForMember(
                dest => dest.DateOfBirth,
                opt => opt.Ignore()
            );
        CreateMap<ApplicantInfoDto, Applicant>();

    }
    
}