using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int requestID {  get; set; }
        [Display(Name = "Associated Contract")]
        public int contractID { get; set; }
        [Display(Name = "Request Description")]
        public string description { get; set; }
        [Display(Name = "Converted Cost (ZAR)")]
        [Precision(18,2)]
        public decimal costinZAR { get; set; }

        [Display(Name = "Original Cost")]
        [Precision(18, 2)]
        public decimal originalCost { get; set; }
        [Display(Name = "Linked Client Company")]
        public string? clientCompanyName { get; set; }

        [Display(Name = "Contract Reference Info")]
        public string? contractDisplayInfo { get; set; }
        [Display(Name = "Progress Status")]
        publicrRequestStatus status { get; set; }
       
        //service request statuses for UI
        public enum requestStatus
        {
            Pending,
            InTransit,
            Delivered,
            Cancelled
        }

        //no relationships because API manages that
        
    }
}
