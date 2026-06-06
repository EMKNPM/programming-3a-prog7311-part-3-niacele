using GLMS.API.Data;
using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural
{
    public class ServiceRequestManager
    { //observer list holds all subscribed background tasks
        private readonly List<IServiceRequestObserver> _observers = new List<IServiceRequestObserver>();

        private readonly GLMSContext _context;

        public ServiceRequestManager(GLMSContext context)
        {
            _context = context;

            //registers the currency converter
            Attach(new CurrencyConversionObserver(context));
        }

        //permits the addition of subscribers - in this case, its the currency converter API
        public void Attach(IServiceRequestObserver observer)
        {
            _observers.Add(observer);
        }

        public async Task CreateServiceRequestAsync(Contract parentContract, ServiceRequest request)
        {
            
            //checks contract status - if expired, throws error 
            parentContract.CreateServiceRequest(request);

            //save to database if above line passes
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            //broadcasts to all subscribers - currency converter API
            await NotifyObserversAsync(request);
        }

        private async Task NotifyObserversAsync(ServiceRequest request)
        {
            foreach (var observer in _observers)
            {
                //triggers currency converter API
                await observer.UpdateAsync(request);
            }
        }
    }
}
