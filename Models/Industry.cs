using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.Models
{
    public class Industry: BaseEntity
    {
        [Required]
        public string Name { get; set; }

        //Navigation
        //public virtual Company Company { get; set; }
    }
}