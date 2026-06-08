using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public class OnHoldState :IContractState
    {
        public void HandleServiceRequest(Contract contract, ServiceRequest request)
        {
            throw new InvalidOperationException("Action Denied: Cannot add service requests to contracts on hold.");

        }
    }
}
