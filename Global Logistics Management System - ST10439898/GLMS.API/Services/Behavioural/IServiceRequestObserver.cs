using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public interface IServiceRequestObserver
    {
        Task UpdateAsync(ServiceRequest request);


    }
}
