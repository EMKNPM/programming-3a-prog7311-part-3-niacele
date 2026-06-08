using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    //state interface that acts as the blueprint
    public interface IContractState
    {
        void HandleServiceRequest(Contract context, ServiceRequest request);
    }
}
