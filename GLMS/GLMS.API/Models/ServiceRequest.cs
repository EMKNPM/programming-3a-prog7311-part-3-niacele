using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class ServiceRequest
    {
        [Key]
        public int requestID {  get; set; }
        public int contractID { get; set; }
        public string Description { get; set; }
        //for the database - 'Cost' is for currency
        [Precision(18,2)]
        public decimal CostinZAR { get; set; }

        [Precision(18, 2)]
        public decimal OriginalCost { get; set; }
        public Status requestStatus { get; set; }
       
        //service request statuses
        public enum Status
        {
            Pending,
            InTransit,
            Delivered,
            Cancelled
        }

        //relationships + navigation
        [ForeignKey("contractID")]
        public virtual Contract Contract { get; set; }
    }
}
