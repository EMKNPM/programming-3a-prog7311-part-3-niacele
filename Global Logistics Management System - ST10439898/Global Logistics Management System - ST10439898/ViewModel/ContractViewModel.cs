using GLMS.API.Services.Behavioural;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class ContractViewModel
    {
        public int contractID { get; set; }
        [Display(Name = "Target Client")]
        public int clientID { get; set; }
        [Display(Name = "Start Date")]
        public DateTime startDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime endDate { get; set; }
        [Display(Name = "Contract Status")]
        public string contractStatus { get; set; }
        [Display(Name = "Service Level Tier")]
        public string serviceLevel { get; set; }
        [Display(Name = "Signed Document Path Location")]
        public string? signedAgreementPath { get; set; }
        [Display(Name = "Last Modified Timestamp")]
        public DateTime? lastModified { get; set; }

        [Display(Name = "Upload Signed Contract Agreement (PDF Only)")]
        public IFormFile? pdfUpload { get; set; }

        [Display(Name = "Associated Client/Company Name")]
        public string? clientCompanyName { get; set; }

        //no relationship or design pattern methods because the API manages those

    }
}