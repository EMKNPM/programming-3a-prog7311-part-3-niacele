using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class ClientViewModel
    {
        public int clientID { get; set;  }
        [Display(Name = "Client Name")]
        public string clientName { get; set; }
        [Display(Name = "Email Address")]
        public string clientEmail { get; set; }
        [Display(Name = "Logistics Region Zone")]
        public string clientRegion { get; set; }
        [Display(Name = "Company Name")]
        public string companyName { get; set; }

        //no relationship attributes because this is handled in the API


     }
}
