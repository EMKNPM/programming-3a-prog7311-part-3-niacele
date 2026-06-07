using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Management_System___ST10439898.API.Models
{
    public class ClientViewModel
    {
        public int ClientID { get; set;  }
        [Display(name = "Client Name")]
        public string ClientName { get; set; }
        [Display(name = "Email Address")]
        public string ClientEmail { get; set; }
        [Display(name = "Logistics Region Zone")]
        public string ClientRegion { get; set; }
        [Display(name = "Company Name")]
        public string CompanyName { get; set; }

        //no relationship attributes because this is handled in the API


     }
}
