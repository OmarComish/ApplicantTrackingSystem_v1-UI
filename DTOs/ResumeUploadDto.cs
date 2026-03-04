using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.DTOs
{
    public class ResumeUploadResultDto
    {

        public string ResumeUrl { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }

    }
}