using GLMS.API.Services.Behavioural;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class ContractViewModel
    {
        public int ContractID { get; set; }
        [Display(Name = "Target Client")]
        public int ClientID { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Contract Status")]
        public string ContractStatus { get; set; }
        [Display(Name = "Service Level Tier")]
        public string ServiceLevel { get; set; }
        [Display(Name = "Signed Document Path Location")]
        public string? SignedAgreementPath { get; set; }
        [Display(Name = "Last Modified Timestamp")]
        public DateTime? LastModified { get; set; }

        [Display(Name = "Upload Signed Contract Agreement (PDF Only)")]
        public IFormFile? PdfUpload { get; set; }

        [Display(Name = "Associated Client/Company Name")]
        public string? ClientCompanyName { get; set; }

        //no relationship or design pattern methods because the API manages those

    }
}