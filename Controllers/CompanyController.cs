using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ATS.API.DTOs;
using ATS.API.Interfaces;
using ATS.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly ICompanyService _companyservice;

        public CompanyController(ILogger<CompanyController> logger, ICompanyService companyservice)
        {
            _logger = logger;
            _companyservice = companyservice;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
        {
            var response = await _companyservice.GetAllAsync();
            return Ok(response);
        }

    
    }
}