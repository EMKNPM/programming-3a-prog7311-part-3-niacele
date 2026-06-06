using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models
{
    public class Client
    {
        [Key]
        public int clientID { get; set;  }
        public string clientName { get; set; }  
        public string clientEmail { get; set; }
        public string clientRegion { get; set; }
        public string companyName { get; set; }


        //relationship
        public virtual ICollection<Contract>? Contracts { get; set; }
    }
}
