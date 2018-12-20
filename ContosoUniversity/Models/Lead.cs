using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Lead
    {
        public int LeadId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime EmailDate { get; set; }

        [StringLength(250)]
        public string Source { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Request")]
        public DateTime? RequestDate { get; set; }

        [StringLength(250)]
        [Display(Name = "Case Attended By")]
        [RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5\s\/]+$", ErrorMessage = "The {0} must not contains numbers and special characters.")]
        public string CaseAttendedBy { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 3)]
        [Display(Name = "Client Name")]
        [RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5\s\/]+$", ErrorMessage = "The {0} must not contains numbers and special characters.")]
        public string ClientName { get; set; }

        [StringLength(250)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(16)]
        [Display(Name = "Contact Number #1")]
        [RegularExpression(@"^(\(?\+?[0-9]*\)?)?[0-9_\- \(\)]*$", ErrorMessage = "Please enter a valid contact number. e.g. 91234567 or +65 12345678 or (65)12345678")]
        public string ContactNumber1 { get; set; }

        [StringLength(16)]
        [Display(Name = "Contact Number #2")]
        [RegularExpression(@"^(\(?\+?[0-9]*\)?)?[0-9_\- \(\)]*$", ErrorMessage = "Please enter a valid contact number. e.g. 91234567 or +65 12345678 or (65)12345678")]
        public string ContactNumber2 { get; set; }

        public int ServiceRequestID { get; set; }

        public int CountryLicenseTypeID { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Follow Up Date #1")]
        public DateTime? FollowUpDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Follow Up Date #2")]
        public DateTime? FollowUpDate2 { get; set; }

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        public int LeadCreatedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        public int? LeadLastUpdatedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdatedDate { get; set; }

        public bool isDeleted { get; set; }
        public int? DeletedByUserId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DeletedDate { get; set; }

        public virtual ServiceRequest ServiceRequester { get; set; }
        public virtual CountryLicenseType CountryLicense { get; set; }
    }
}