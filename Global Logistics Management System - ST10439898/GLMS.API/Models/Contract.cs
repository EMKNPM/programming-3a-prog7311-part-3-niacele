using GLMS.API.Services.Behavioural;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class Contract
    {
        [Key]
        public int contractID { get; set; }
        public int clientID { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Status contractStatus { get; set; }
        public string serviceLevel { get; set; }
        public string signedAgreementPath { get; set; }
        public DateTime? LastModified { get; set; }

        //contract statuses
        public enum Status
        {
            Draft,
            Active,
            Expired,
            OnHold
        }

        //relationships + navigation
        [ForeignKey("clientID")]
        public virtual Client? Client { get; set; }
        public virtual ICollection<ServiceRequest>? ServiceRequests { get; set; }

        /*Below is code that'll be used by the State Design Pattern's implementation*/

        //current contract state
        [NotMapped]
        private IContractState? _currentState;

        //sets the default state to default
        public Contract()
        {
            _currentState = new DraftState();
            ServiceRequests = new HashSet<ServiceRequest>();
            contractStatus = Status.Draft;
        }

        //sync state from database status
        public void SyncStateFromStatus()
        {
            _currentState = contractStatus switch
            {
                Status.Draft => new DraftState(),
                Status.Active => new ActiveState(),
                Status.OnHold => new OnHoldState(),
                Status.Expired => new ExpiredState(),
                _ => new DraftState()
            };
        }

        //lets the states change the status
        public void SetState(IContractState state)
        {
            this._currentState = state;

            //retrieves the name of the state class
            string className = state.GetType().Name;

            //matches the enum
            string enumName = className.Replace("State", "");

            //parse string to enum + assigns it
            this.contractStatus = (Status)Enum.Parse(typeof(Status), enumName);
        }

        //interfaces between the main project and the states
        public void CreateServiceRequest(ServiceRequest request)
        {
            if (_currentState == null)
            {
                SyncStateFromStatus();
            }
            _currentState.HandleServiceRequest(this, request);
        }
    }
}