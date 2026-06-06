using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public class DraftState : IContractState
    {
        public void HandleServiceRequest (Contract context, ServiceRequest request)
        {
            throw new InvalidOperationException ("Action Denied: Cannot add a request to a contract that is in draft mode.");
        }
    }
}
