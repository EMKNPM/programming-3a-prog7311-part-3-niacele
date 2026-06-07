using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int RequestID {  get; set; }
        [Display(Name = "Associated Contract")]
        public int ContractID { get; set; }
        [Display(Name = "Request Description")]
        public string Description { get; set; }
        [Display(Name = "Converted Cost (ZAR)")]
        [Precision(18,2)]
        public decimal CostinZAR { get; set; }

        [Display(Name = "Original Cost")]
        [Precision(18, 2)]
        public decimal OriginalCost { get; set; }
        [Display(Name = "Linked Client Company")]
        public string? ClientCompanyName { get; set; }

        [Display(Name = "Contract Reference Info")]
        public string? ContractDisplayInfo { get; set; }
        [Display(Name = "Progress Status")]
        public RequestStatus status { get; set; }
       
        //service request statuses for UI
        public enum RequestStatus
        {
            Pending,
            InTransit,
            Delivered,
            Cancelled
        }

        //no relationships because API manages that
        
    }
}
