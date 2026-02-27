using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.Models
{
    public class Company: BaseEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int IndustryId { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Description { get; set; }
        public string Logo {get; set;}

        //Navigation properties
        public virtual Industry Industry { get; set; }
    }
}