using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public class ActiveState : IContractState
    {
        //request gets added because status is active
        public void HandleServiceRequest (Contract context, ServiceRequest request)
        {
            context.ServiceRequests.Add (request);

            
        }

    }
}
