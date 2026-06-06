using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public class ExpiredState : IContractState
    {
        //error handling - can only add service requests to active contracts
        public void HandleServiceRequest (Contract contract, ServiceRequest request)
        {
            throw new InvalidOperationException("Action Denied: Cannot add service requests to expired contracts."); 

        }

    }
}
