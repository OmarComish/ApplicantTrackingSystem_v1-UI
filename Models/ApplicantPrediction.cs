using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace ATS.API.Models
{
    public class ApplicantPrediction :BaseEntity
    {
        [ColumnName("Score")]
        public float PredictedScore { get; set; }
    }
}