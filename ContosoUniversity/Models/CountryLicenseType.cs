using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class CountryLicenseType
    {
        public int CountryLicenseTypeId { get; set; }

        [StringLength(250)]
        [Display(Name = "Country / License Name")]
        public string CountryLicenseTypeName { get; set; }

        public int CountryLicenseTypeCreatedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        public int? LastUpdatedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdatedDate { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public int? DeletedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DeletedDate { get; set; }
        
        public bool IsVisible { get; set; }
    }
}