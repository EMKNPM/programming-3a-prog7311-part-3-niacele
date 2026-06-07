using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class ClientViewModel
    {
        public int cientId { get; set;  }
        [Display(name = "Client Name")]
        public string clientName { get; set; }
        [Display(name = "Email Address")]
        public string clientEmail { get; set; }
        [Display(name = "Logistics Region Zone")]
        public string clientRegion { get; set; }
        [Display(name = "Company Name")]
        public string companyName { get; set; }

        //no relationship attributes because this is handled in the API


     }
}
